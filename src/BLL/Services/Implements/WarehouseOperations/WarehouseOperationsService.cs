using BLL.DTOs;
using BLL.Services.Interfaces.WarehouseOperations;
using DAL;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services.Implements.WarehouseOperations;

public class WarehouseOperationsService(AppDbContext context) : IWarehouseOperationsService
{
    public async Task<WarehouseLayoutDto> GetLayoutAsync(Guid staffId)
    {
        var staffWarehouseId = await context.Users.AsNoTracking()
            .Where(x => x.Id == staffId).Select(x => x.WarehouseId).FirstOrDefaultAsync();
        var warehouse = staffWarehouseId.HasValue
            ? await context.Warehouses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == staffWarehouseId && x.IsActive != false)
            : await context.Warehouses.AsNoTracking().FirstOrDefaultAsync(x => x.IsActive != false);
        if (warehouse is null) throw new InvalidOperationException("Warehouse not found for this staff account.");
        await EnsureDefaultLayoutAsync(warehouse.Id);

        var areas = await context.WarehouseAreas.AsNoTracking()
            .Where(x => x.WarehouseId == warehouse.Id && x.IsActive != false)
            .OrderBy(x => x.AreaName).ToListAsync();
        var groups = await context.AreaGroups.AsNoTracking()
            .Where(x => areas.Select(a => a.Id).Contains(x.AreaId) && x.IsActive != false)
            .OrderBy(x => x.GroupName).ToListAsync();
        var locations = await context.StorageLocations.AsNoTracking()
            .Where(x => x.WarehouseId == warehouse.Id && x.IsActive != false)
            .OrderBy(x => x.AisleCode).ThenBy(x => x.RackCode).ThenBy(x => x.ShelfCode).ThenBy(x => x.BinCode)
            .ToListAsync();
        var inventoryStats = await context.Inventories.AsNoTracking()
            .Where(x => x.WarehouseId == warehouse.Id && x.StorageLocationId.HasValue && x.IsActive != false)
            .GroupBy(x => x.StorageLocationId!.Value)
            .Select(x => new { LocationId = x.Key, Count = x.Count(), Quantity = x.Sum(i => i.Quantity) })
            .ToDictionaryAsync(x => x.LocationId);

        var areaDtos = areas.Select(area => new WarehouseAreaLayoutDto(
            area.Id, area.AreaName, area.Description, area.CapacityKg, area.CurrentKg,
            groups.Where(x => x.AreaId == area.Id).Select(x => new WarehouseGroupLayoutDto(
                x.Id, x.GroupName, x.Description, x.CapacityKg, x.CurrentKg)).ToList(),
            locations.Where(x => x.AreaId == area.Id).Select(x =>
            {
                inventoryStats.TryGetValue(x.Id, out var stats);
                return new WarehouseLocationLayoutDto(x.Id, x.LocationCode, x.AisleCode, x.RackCode,
                    x.ShelfCode, x.BinCode, x.PreferredGarmentGroup, x.PreferredProcessingDirection,
                    x.CapacityKg, x.CurrentWeightKg, x.Status, stats?.Count ?? 0, stats?.Quantity ?? 0);
            }).ToList())).ToList();
        return new WarehouseLayoutDto(warehouse.Id, warehouse.WarehouseName, warehouse.Address,
            warehouse.TotalCapacityKg, warehouse.CurrentWeight, areaDtos);
    }

    public async Task<WarehouseDashboardDto> GetDashboardAsync()
    {
        var pending = await context.ClassifiedBatches.CountAsync(x => x.IsActive != false && x.Status == "PendingWarehouseReceipt");
        var putaway = await context.ClassifiedBatches.CountAsync(x => x.IsActive != false && x.Status == "WarehouseReceived");
        var stored = await context.ClassifiedBatches.CountAsync(x => x.IsActive != false && x.Status == "Stored");
        var inventory = await context.Inventories.AsNoTracking().Where(x => x.IsActive != false).ToListAsync();
        var warehouses = await context.Warehouses.AsNoTracking().Where(x => x.IsActive != false).ToListAsync();
        var capacity = warehouses.Sum(x => x.TotalCapacityKg);
        var current = warehouses.Sum(x => x.CurrentWeight);
        return new WarehouseDashboardDto(pending, putaway, stored,
            inventory.Sum(x => Math.Max(0, x.Quantity - x.ReservedQuantity)),
            inventory.Count(x => Math.Max(0, x.Quantity - x.ReservedQuantity) > 0),
            inventory.Sum(x => Math.Max(0, x.TotalWeight - x.ReservedWeight)),
            capacity <= 0 ? 0 : Math.Round(current / capacity * 100, 2));
    }

    public async Task<IReadOnlyList<WarehouseInboundBatchDto>> GetInboundBatchesAsync()
    {
        var batches = await BatchQuery()
            .Where(x => x.Status == "PendingWarehouseReceipt" || x.Status == "WarehouseReceived" || x.Status == "Stored")
            .OrderByDescending(x => x.SentToWarehouseAt ?? x.ClassificationDate)
            .ToListAsync();
        return batches.Select(MapBatch).ToList();
    }

    public async Task<WarehouseInboundBatchDto?> GetBatchAsync(Guid batchId)
    {
        var batch = await BatchQuery().FirstOrDefaultAsync(x => x.Id == batchId);
        return batch is null ? null : MapBatch(batch);
    }

    public async Task ConfirmReceiptAsync(Guid staffId, Guid batchId, ConfirmWarehouseReceiptDto dto)
    {
        if (dto.ActualItemCount <= 0 || dto.ActualWeightKg <= 0)
            throw new InvalidOperationException("Actual item count and weight must be greater than zero.");
        if (!dto.SealIntact && string.IsNullOrWhiteSpace(dto.DiscrepancyNotes))
            throw new InvalidOperationException("A discrepancy note is required when the seal is not intact.");

        await using var transaction = await context.Database.BeginTransactionAsync();
        var batch = await context.ClassifiedBatches.Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == batchId && x.IsActive != false)
            ?? throw new InvalidOperationException("Classified batch not found.");
        if (batch.Status != "PendingWarehouseReceipt")
            throw new InvalidOperationException("Only a batch pending warehouse receipt can be confirmed.");

        var expectedCount = batch.Items.Count(x => x.IsActive != false);
        var notes = BuildReceiptNotes(expectedCount, dto);
        batch.Status = "WarehouseReceived";
        batch.WarehouseReceivedAt = DateTime.UtcNow;
        batch.WarehouseReceivedByStaffId = staffId;
        batch.ReceivedItemCount = dto.ActualItemCount;
        batch.ReceivedWeight = dto.ActualWeightKg;
        batch.WarehouseReceiptNotes = notes;
        batch.TotalItem = dto.ActualItemCount;
        batch.TotalWeight = dto.ActualWeightKg;
        batch.UpdateAt = DateTime.UtcNow;
        batch.UpdatedBy = staffId;

        var inventory = new Inventory
        {
            Id = Guid.NewGuid(), WarehouseId = batch.WarehouseId, ClassifiedBatchId = batch.Id,
            FabricTypeId = batch.FabricTypeId, GarmentGroupId = batch.GarmentGroupId,
            ClothingTypeId = batch.ClothingTypeId, GenderId = batch.GenderId,
            TargetUserId = batch.TargetUserId, SizeId = batch.SizeId,
            ConditionGradeId = batch.ConditionGradeId,
            Sku = $"SKU-{batch.BatchCode}", FabricType = batch.FabricType,
            GarmentGroup = batch.GarmentGroup, ClothingType = batch.ClothingType,
            Gender = batch.Gender, TargetUser = batch.TargetUser, Size = batch.Size,
            ProcessingDirection = batch.ProcessingDirection, ConditionRating = batch.ConditionRating,
            Quantity = dto.ActualItemCount, TotalWeight = dto.ActualWeightKg,
            Status = "AwaitingPutaway", CreateAt = DateTime.UtcNow, CreatedBy = staffId
        };
        context.Inventories.Add(inventory);
        AddTransaction(staffId, batch.WarehouseId, "RECEIPT", "ClassifiedBatch", batch.Id,
            notes, inventory, dto.ActualItemCount, dto.ActualWeightKg, 0, dto.ActualItemCount,
            0, dto.ActualWeightKg, null, null);
        await context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task<IReadOnlyList<StorageLocationDto>> GetLocationsAsync(Guid batchId)
    {
        var batch = await context.ClassifiedBatches.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == batchId && x.IsActive != false)
            ?? throw new InvalidOperationException("Classified batch not found.");
        await EnsureDefaultLayoutAsync(batch.WarehouseId);
        var locations = await context.StorageLocations.AsNoTracking()
            .Include(x => x.Area)
            .Where(x => x.WarehouseId == batch.WarehouseId && x.IsActive != false && x.Status != "Blocked")
            .ToListAsync();
        return locations.Select(x => MapLocation(x, batch)).OrderByDescending(x => x.MatchScore)
            .ThenBy(x => x.LocationCode).ToList();
    }

    public async Task PutawayAsync(Guid staffId, Guid batchId, PutawayBatchDto dto)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        var batch = await context.ClassifiedBatches
            .FirstOrDefaultAsync(x => x.Id == batchId && x.IsActive != false)
            ?? throw new InvalidOperationException("Classified batch not found.");
        if (batch.Status != "WarehouseReceived")
            throw new InvalidOperationException("Confirm physical receipt before putaway.");
        var inventory = await context.Inventories
            .FirstOrDefaultAsync(x => x.ClassifiedBatchId == batchId && x.IsActive != false)
            ?? throw new InvalidOperationException("Receiving inventory record not found.");
        var location = await context.StorageLocations
            .Include(x => x.Area).Include(x => x.Warehouse)
            .FirstOrDefaultAsync(x => x.Id == dto.LocationId && x.WarehouseId == batch.WarehouseId && x.IsActive != false)
            ?? throw new InvalidOperationException("Storage location not found.");
        if (location.Status == "Blocked")
            throw new InvalidOperationException("Storage location is blocked.");
        if (location.CapacityKg - location.CurrentWeightKg < inventory.TotalWeight)
            throw new InvalidOperationException("Storage location does not have enough remaining capacity.");

        inventory.StorageLocationId = location.Id;
        inventory.AreaGroupId = location.AreaGroupId;
        inventory.Status = "Available";
        inventory.UpdateAt = DateTime.UtcNow;
        inventory.UpdatedBy = staffId;
        location.CurrentWeightKg += inventory.TotalWeight;
        location.Area.CurrentKg += inventory.TotalWeight;
        location.Warehouse.CurrentWeight += inventory.TotalWeight;
        batch.AreaId = location.AreaId;
        batch.GroupId = location.AreaGroupId;
        batch.Status = "Stored";
        batch.StoredAt = DateTime.UtcNow;
        batch.StoredByStaffId = staffId;
        batch.UpdateAt = DateTime.UtcNow;
        batch.UpdatedBy = staffId;

        AddTransaction(staffId, batch.WarehouseId, "PUTAWAY", "ClassifiedBatch", batch.Id,
            dto.Notes, inventory, inventory.Quantity, inventory.TotalWeight,
            inventory.Quantity, inventory.Quantity, inventory.TotalWeight, inventory.TotalWeight,
            null, location.Id);
        await context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task<IReadOnlyList<WarehouseInventoryDto>> GetInventoryAsync(string? search)
    {
        var query = context.Inventories.AsNoTracking()
            .Include(x => x.ClassifiedBatch).Include(x => x.StorageLocation)!.ThenInclude(x => x!.Area)
            .Where(x => x.IsActive != false);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Sku.Contains(term) || x.ClothingType.Contains(term)
                || (x.StorageLocation != null && x.StorageLocation.LocationCode.Contains(term)));
        }
        return await query.OrderBy(x => x.StorageLocation!.LocationCode).ThenBy(x => x.Sku)
            .Select(x => new WarehouseInventoryDto(x.Id, x.Sku, x.ClassifiedBatchId!.Value,
                x.ClassifiedBatch!.BatchCode, x.StorageLocation != null ? x.StorageLocation.LocationCode : "RECEIVING",
                x.StorageLocation != null ? x.StorageLocation.Area.AreaName : "Khu tiếp nhận",
                x.FabricType, x.GarmentGroup, x.ClothingType, x.Gender, x.TargetUser, x.Size,
                Grade(x.ConditionRating), x.ProcessingDirection, x.Quantity, x.ReservedQuantity,
                x.Quantity - x.ReservedQuantity, x.TotalWeight, x.ReservedWeight,
                x.TotalWeight - x.ReservedWeight, x.Status, x.ClassifiedBatch.StoredAt)).ToListAsync();
    }

    public async Task<IReadOnlyList<WarehouseTransactionDto>> GetTransactionsAsync(string? type)
    {
        var query = context.InventoryTransactions.AsNoTracking()
            .Include(x => x.PerformedByStaff).Include(x => x.Items).ThenInclude(x => x.Inventory)
            .Include(x => x.Items).ThenInclude(x => x.ClassifiedBatch)
            .Include(x => x.Items).ThenInclude(x => x.SourceLocation)
            .Include(x => x.Items).ThenInclude(x => x.DestinationLocation)
            .Where(x => x.IsActive != false);
        if (!string.IsNullOrWhiteSpace(type)) query = query.Where(x => x.TransactionType == type);
        var transactions = await query.OrderByDescending(x => x.PerformedAt).Take(200).ToListAsync();
        return transactions.Select(x => new WarehouseTransactionDto(x.Id, x.TransactionCode,
            x.TransactionType, x.ReferenceType, x.ReferenceId, x.Status, x.Notes, x.PerformedAt,
            x.PerformedByStaff.FullName, x.Items.Select(i => new WarehouseTransactionItemDto(i.Id,
                i.InventoryId, i.Inventory.Sku, i.ClassifiedBatch?.BatchCode, i.Quantity, i.Weight,
                i.QuantityBefore, i.QuantityAfter, i.WeightBefore, i.WeightAfter,
                i.SourceLocation?.LocationCode, i.DestinationLocation?.LocationCode, i.Notes)).ToList())).ToList();
    }

    public async Task IssueAsync(Guid staffId, Guid inventoryId, IssueInventoryDto dto)
    {
        if (dto.Quantity <= 0 || dto.WeightKg <= 0 || string.IsNullOrWhiteSpace(dto.Reason))
            throw new InvalidOperationException("Quantity, weight and issue reason are required.");
        await using var transaction = await context.Database.BeginTransactionAsync();
        var inventory = await InventoryForMutation(inventoryId);
        if (inventory.Status != "Available") throw new InvalidOperationException("Inventory is not available for issue.");
        if (inventory.Quantity - inventory.ReservedQuantity < dto.Quantity
            || inventory.TotalWeight - inventory.ReservedWeight < dto.WeightKg)
            throw new InvalidOperationException("Requested issue exceeds available inventory.");
        var beforeQuantity = inventory.Quantity;
        var beforeWeight = inventory.TotalWeight;
        inventory.Quantity -= dto.Quantity;
        inventory.TotalWeight -= dto.WeightKg;
        inventory.Status = inventory.Quantity == 0 ? "Depleted" : "Available";
        inventory.UpdateAt = DateTime.UtcNow;
        inventory.UpdatedBy = staffId;
        AdjustLocationWeight(inventory, -dto.WeightKg);
        AddTransaction(staffId, inventory.WarehouseId, "OUT", dto.ReferenceType, dto.ReferenceId,
            $"{dto.Reason}. {dto.Notes}".Trim(), inventory, dto.Quantity, dto.WeightKg,
            beforeQuantity, inventory.Quantity, beforeWeight, inventory.TotalWeight,
            inventory.StorageLocationId, null);
        await context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task MoveAsync(Guid staffId, Guid inventoryId, MoveInventoryDto dto)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();
        var inventory = await InventoryForMutation(inventoryId);
        if (!inventory.StorageLocationId.HasValue) throw new InvalidOperationException("Inventory has not been put away.");
        if (inventory.StorageLocationId == dto.DestinationLocationId) throw new InvalidOperationException("Destination must differ from source.");
        var destination = await context.StorageLocations.Include(x => x.Area).Include(x => x.Warehouse)
            .FirstOrDefaultAsync(x => x.Id == dto.DestinationLocationId && x.WarehouseId == inventory.WarehouseId && x.IsActive != false)
            ?? throw new InvalidOperationException("Destination location not found.");
        if (destination.CapacityKg - destination.CurrentWeightKg < inventory.TotalWeight)
            throw new InvalidOperationException("Destination location does not have enough capacity.");
        var sourceId = inventory.StorageLocationId;
        AdjustLocationWeight(inventory, -inventory.TotalWeight);
        destination.CurrentWeightKg += inventory.TotalWeight;
        destination.Area.CurrentKg += inventory.TotalWeight;
        destination.Warehouse.CurrentWeight += inventory.TotalWeight;
        inventory.StorageLocationId = destination.Id;
        inventory.AreaGroupId = destination.AreaGroupId;
        inventory.UpdateAt = DateTime.UtcNow;
        inventory.UpdatedBy = staffId;
        AddTransaction(staffId, inventory.WarehouseId, "MOVE", "Inventory", inventory.Id,
            $"{dto.Reason}. {dto.Notes}".Trim(), inventory, inventory.Quantity, inventory.TotalWeight,
            inventory.Quantity, inventory.Quantity, inventory.TotalWeight, inventory.TotalWeight,
            sourceId, destination.Id);
        await context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    private IQueryable<ClassifiedBatch> BatchQuery() => context.ClassifiedBatches.AsNoTracking()
        .Include(x => x.Items.Where(i => i.IsActive != false))
        .Where(x => x.IsActive != false);

    private async Task<Inventory> InventoryForMutation(Guid id) => await context.Inventories
        .Include(x => x.StorageLocation)!.ThenInclude(x => x!.Area)
        .Include(x => x.StorageLocation)!.ThenInclude(x => x!.Warehouse)
        .FirstOrDefaultAsync(x => x.Id == id && x.IsActive != false)
        ?? throw new InvalidOperationException("Inventory not found.");

    private void AdjustLocationWeight(Inventory inventory, decimal delta)
    {
        if (inventory.StorageLocation is null) return;
        inventory.StorageLocation.CurrentWeightKg += delta;
        inventory.StorageLocation.Area.CurrentKg += delta;
        inventory.StorageLocation.Warehouse.CurrentWeight += delta;
    }

    private void AddTransaction(Guid staffId, Guid warehouseId, string type, string? referenceType,
        Guid? referenceId, string? notes, Inventory inventory, int quantity, decimal weight,
        int quantityBefore, int quantityAfter, decimal weightBefore, decimal weightAfter,
        Guid? sourceLocationId, Guid? destinationLocationId)
    {
        var now = DateTime.UtcNow;
        context.InventoryTransactions.Add(new InventoryTransaction
        {
            Id = Guid.NewGuid(), WarehouseId = warehouseId,
            TransactionCode = $"TX-{type}-{now:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..32].ToUpperInvariant(),
            TransactionType = type, ReferenceType = referenceType, ReferenceId = referenceId,
            Status = "Posted", Notes = notes, PerformedByStaffId = staffId, PerformedAt = now,
            CreateAt = now, CreatedBy = staffId,
            Items =
            [
                new TransactionItem
                {
                    Id = Guid.NewGuid(), InventoryId = inventory.Id,
                    ClassifiedBatchId = inventory.ClassifiedBatchId, Quantity = quantity, Weight = weight,
                    QuantityBefore = quantityBefore, QuantityAfter = quantityAfter,
                    WeightBefore = weightBefore, WeightAfter = weightAfter,
                    SourceLocationId = sourceLocationId, DestinationLocationId = destinationLocationId,
                    Notes = notes, CreateAt = now, CreatedBy = staffId
                }
            ]
        });
    }

    private async Task EnsureDefaultLayoutAsync(Guid warehouseId)
    {
        if (await context.StorageLocations.AnyAsync(x => x.WarehouseId == warehouseId && x.IsActive != false)) return;
        var definitions = new[]
        {
            ("CHARITY", "Khu hàng từ thiện", "Charity"),
            ("RECYCLE", "Khu hàng tái chế", "Recycling"),
            ("DISPOSAL", "Khu cách ly/tiêu hủy", "Disposal")
        };
        foreach (var (areaCode, areaName, direction) in definitions)
        {
            var area = new WarehouseArea
            {
                Id = Guid.NewGuid(), WarehouseId = warehouseId, AreaName = areaName,
                Description = $"Khu vực kiểm soát cho hướng xử lý {direction}", CapacityKg = 5000,
                CurrentKg = 0, CreateAt = DateTime.UtcNow, IsActive = true
            };
            var group = new AreaGroup
            {
                Id = Guid.NewGuid(), AreaId = area.Id, GroupName = $"Dãy {areaCode}-A",
                Description = "Dãy lưu trữ tiêu chuẩn", CapacityKg = 5000, CurrentKg = 0,
                CreateAt = DateTime.UtcNow, IsActive = true
            };
            context.WarehouseAreas.Add(area);
            context.AreaGroups.Add(group);
            for (var rack = 1; rack <= 2; rack++)
            for (var shelf = 1; shelf <= 3; shelf++)
            {
                var code = $"{areaCode}-A01-R{rack:00}-S{shelf:00}-B01";
                context.StorageLocations.Add(new StorageLocation
                {
                    Id = Guid.NewGuid(), WarehouseId = warehouseId, AreaId = area.Id,
                    AreaGroupId = group.Id, LocationCode = code, AisleCode = "A01",
                    RackCode = $"R{rack:00}", ShelfCode = $"S{shelf:00}", BinCode = "B01",
                    PreferredProcessingDirection = direction, CapacityKg = 300,
                    Status = "Available", CreateAt = DateTime.UtcNow, IsActive = true
                });
            }
        }
        await context.SaveChangesAsync();
    }

    private static StorageLocationDto MapLocation(StorageLocation x, ClassifiedBatch batch)
    {
        var score = 0;
        if (x.PreferredProcessingDirection == batch.ProcessingDirection) score += 70;
        if (string.IsNullOrWhiteSpace(x.PreferredGarmentGroup) || x.PreferredGarmentGroup == batch.GarmentGroup) score += 20;
        if (x.CapacityKg - x.CurrentWeightKg >= (batch.ReceivedWeight ?? batch.TotalWeight)) score += 10;
        return new StorageLocationDto(x.Id, x.LocationCode, x.Area.AreaName, x.AisleCode,
            x.RackCode, x.ShelfCode, x.BinCode, x.PreferredGarmentGroup,
            x.PreferredProcessingDirection, x.CapacityKg, x.CurrentWeightKg,
            x.CapacityKg - x.CurrentWeightKg, x.Status, score);
    }

    private static WarehouseInboundBatchDto MapBatch(ClassifiedBatch x) => new(x.Id, x.BatchCode,
        x.ClassificationDate, x.FabricType, x.GarmentGroup, x.ClothingType, x.Gender, x.TargetUser,
        x.Size, Grade(x.ConditionRating), x.ProcessingDirection, x.TotalItem, x.TotalWeight, x.Status,
        x.SentToWarehouseAt, x.WarehouseReceivedAt, x.ReceivedWeight, x.ReceivedItemCount,
        x.WarehouseReceiptNotes, x.Items.OrderBy(i => i.ItemCode).Select(i =>
            new ClassificationItemDto(i.Id, i.ItemCode, i.FabricType, i.GarmentGroup,
                i.ClothingType, i.Gender, i.TargetUser, i.Size, Grade(i.ConditionRating),
                i.ProcessingDirection, i.ImageUrls ?? [], i.Notes, i.ClassifiedAt)).ToList());

    private static string BuildReceiptNotes(int expectedCount, ConfirmWarehouseReceiptDto dto)
    {
        var variance = dto.ActualItemCount - expectedCount;
        var seal = dto.SealIntact ? "Seal intact" : "Seal discrepancy";
        return $"{seal}; item variance: {variance:+#;-#;0}. {dto.DiscrepancyNotes}".Trim();
    }

    private static string Grade(int rating) => rating == 1 ? "A" : rating == 2 ? "B" : "C";
}
