namespace BLL.DTOs;

public record WarehouseDashboardDto(int PendingReceipt, int AwaitingPutaway, int StoredBatches,
    int AvailableQuantity, decimal AvailableWeightKg, decimal CapacityUsedPercent);

public record WarehouseInboundBatchDto(Guid Id, string BatchCode, DateTime ClassificationDate,
    string FabricType, string GarmentGroup, string ClothingType, string Gender, string TargetUser,
    string Size, string ConditionGrade, string ProcessingDirection, int ExpectedItemCount,
    decimal ExpectedWeightKg, string Status, DateTime? SentAt, DateTime? ReceivedAt,
    decimal? ReceivedWeightKg, int? ReceivedItemCount, string? ReceiptNotes,
    IReadOnlyList<ClassificationItemDto> Items);

public record ConfirmWarehouseReceiptDto(decimal ActualWeightKg, int ActualItemCount,
    bool SealIntact, string? DiscrepancyNotes);

public record StorageLocationDto(Guid Id, string LocationCode, string AreaName, string AisleCode,
    string RackCode, string ShelfCode, string BinCode, string? PreferredGarmentGroup,
    string? PreferredProcessingDirection, decimal CapacityKg, decimal CurrentWeightKg,
    decimal AvailableCapacityKg, string Status, int MatchScore);

public record PutawayBatchDto(Guid LocationId, string? Notes);
public record IssueInventoryDto(int Quantity, decimal WeightKg, string Reason,
    string? ReferenceType, Guid? ReferenceId, string? Notes);
public record MoveInventoryDto(Guid DestinationLocationId, string Reason, string? Notes);

public record WarehouseInventoryDto(Guid Id, string Sku, Guid ClassifiedBatchId, string BatchCode,
    string LocationCode, string AreaName, string FabricType, string GarmentGroup,
    string ClothingType, string Gender, string TargetUser, string Size, string ConditionGrade,
    string ProcessingDirection, int Quantity, int ReservedQuantity, int AvailableQuantity,
    decimal TotalWeightKg, decimal ReservedWeightKg, decimal AvailableWeightKg, string Status,
    DateTime? StoredAt);

public record WarehouseTransactionItemDto(Guid Id, Guid InventoryId, string Sku,
    string? ClassifiedBatchCode, int Quantity, decimal WeightKg, int QuantityBefore,
    int QuantityAfter, decimal WeightBefore, decimal WeightAfter, string? SourceLocationCode,
    string? DestinationLocationCode, string? Notes);

public record WarehouseTransactionDto(Guid Id, string TransactionCode, string TransactionType,
    string? ReferenceType, Guid? ReferenceId, string Status, string? Notes,
    DateTime PerformedAt, string PerformedBy, IReadOnlyList<WarehouseTransactionItemDto> Items);
