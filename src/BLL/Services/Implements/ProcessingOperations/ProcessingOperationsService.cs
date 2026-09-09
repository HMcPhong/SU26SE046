using BLL.DTOs;
using BLL.Services.Interfaces.ProcessingOperations;
using DAL;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services.Implements.ProcessingOperations;

public class ProcessingOperationsService(AppDbContext context)
    : IProcessingOperationsService
{
    public async Task<ProcessingOperationCreatedDto> CreateAsync(
        Guid managerId,
        CreateProcessingOperationDto dto)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        ValidateOperationType(dto.OperationType);

        if (dto.Inputs is null || dto.Inputs.Count == 0)
            throw new InvalidOperationException(
                "Select at least one inventory item.");

        var inputIds = dto.Inputs
            .Select(x => x.InventoryId)
            .Distinct()
            .ToList();

        if (inputIds.Count != dto.Inputs.Count)
            throw new InvalidOperationException(
                "An inventory item can only appear once.");

        var warehouse = await context.Warehouses
            .FirstOrDefaultAsync(x =>
                x.Id == dto.WarehouseId &&
                x.IsActive != false);

        if (warehouse is null)
            throw new KeyNotFoundException("Warehouse not found.");

        var organization = await context.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x =>
                x.Id == dto.OrganizationId &&
                x.IsActive != false);

        if (organization is null)
            throw new KeyNotFoundException(
                "Processing organization not found.");

        if (organization.Role.RoleName != "RecyclingOrganization"
            && organization.Role.RoleName != "DisposalOrganization")
        {
            throw new InvalidOperationException(
                "Selected organization is not a valid processing organization.");
        }

        if (dto.OperationType == "Recycling"
            && organization.Role.RoleName != "RecyclingOrganization")
        {
            throw new InvalidOperationException(
                "Recycling operations must use a RecyclingOrganization.");
        }

        if (dto.OperationType == "Disposal"
            && organization.Role.RoleName != "DisposalOrganization")
        {
            throw new InvalidOperationException(
                "Disposal operations must use a DisposalOrganization.");
        }

        var inventories = await context.Inventories
            .Include(x => x.ClassifiedBatch)
            .Where(x =>
                inputIds.Contains(x.Id) &&
                x.IsActive != false &&
                x.WarehouseId == dto.WarehouseId)
            .ToListAsync();

        if (inventories.Count != inputIds.Count)
        {
            throw new InvalidOperationException(
                "One or more inventory items are unavailable or belong to another warehouse.");
        }

        foreach (var input in dto.Inputs)
        {
            if (input.RequestedQuantity <= 0)
                throw new InvalidOperationException(
                    "Requested quantity must be greater than zero.");

            if (input.RequestedWeight <= 0)
                throw new InvalidOperationException(
                    "Requested weight must be greater than zero.");

            var inventory = inventories.Single(x =>
                x.Id == input.InventoryId);

            if (inventory.Status != "Available")
            {
                throw new InvalidOperationException(
                    $"Inventory {inventory.Sku} is not available.");
            }

            if (inventory.ProcessingDirection != dto.OperationType)
            {
                throw new InvalidOperationException(
                    $"Inventory {inventory.Sku} is not classified for {dto.OperationType}.");
            }

            var availableQuantity =
                inventory.Quantity - inventory.ReservedQuantity;

            var availableWeight =
                inventory.TotalWeight - inventory.ReservedWeight;

            if (input.RequestedQuantity > availableQuantity)
            {
                throw new InvalidOperationException(
                    $"Insufficient inventory quantity for {inventory.Sku}.");
            }

            if (input.RequestedWeight > availableWeight)
            {
                throw new InvalidOperationException(
                    $"Insufficient inventory weight for {inventory.Sku}.");
            }
        }

        var lockedInventoryIds = await context.ProcessingOperationInputs
            .Where(input =>
                input.IsActive != false &&
                inputIds.Contains(input.InventoryId) &&
                input.ProcessingOperation.IsActive != false &&
                input.ProcessingOperation.Status != "Rejected" &&
                input.ProcessingOperation.Status != "Cancelled" &&
                input.ProcessingOperation.Status != "Completed")
            .Select(input => input.InventoryId)
            .Distinct()
            .ToListAsync();

        if (lockedInventoryIds.Count > 0)
        {
            throw new InvalidOperationException(
                "One or more inventory items are already assigned to another active processing operation.");
        }

        var operationId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var operation = new ProcessingOperation
        {
            Id = operationId,
            OperationCode = BuildOperationCode(operationId),
            OperationType = dto.OperationType,
            Status = "PendingOrganizationApproval",
            WarehouseId = dto.WarehouseId,
            OrganizationId = dto.OrganizationId,
            CreatedByUserId = managerId,
            RequestedAt = now,
            RequestNotes = dto.RequestNotes?.Trim(),
            CreateAt = now,
            CreatedBy = managerId,
            IsActive = true
        };

        foreach (var input in dto.Inputs)
        {
            var inventory = inventories.Single(x =>
                x.Id == input.InventoryId);

            operation.Inputs.Add(new ProcessingOperationInput
            {
                Id = Guid.NewGuid(),
                ProcessingOperationId = operationId,
                InventoryId = inventory.Id,
                ClassifiedBatchId = inventory.ClassifiedBatchId,
                RequestedQuantity = input.RequestedQuantity,
                RequestedWeight = Math.Round(input.RequestedWeight, 2),
                IssuedQuantity = 0,
                IssuedWeight = 0,
                CreateAt = now,
                CreatedBy = managerId,
                IsActive = true
            });
        }

        context.ProcessingOperations.Add(operation);

        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        return new ProcessingOperationCreatedDto(
            operation.Id,
            operation.OperationCode,
            operation.OperationType,
            operation.Status);
    }

    public async Task ApproveByOrganizationAsync(
        Guid organizationId,
        Guid operationId)
    {
        var operation = await context.ProcessingOperations
            .FirstOrDefaultAsync(x =>
                x.Id == operationId &&
                x.IsActive != false);

        if (operation is null)
            throw new KeyNotFoundException(
                "Processing operation not found.");

        if (operation.OrganizationId != organizationId)
            throw new InvalidOperationException(
                "You are not the organization assigned to this operation.");

        if (operation.Status != "PendingOrganizationApproval")
            throw new InvalidOperationException(
                "This processing operation is not awaiting organization approval.");

        operation.Status = "PendingManagerApproval";
        operation.ApprovedByOrganizationId = organizationId;
        operation.OrganizationRespondedAt = DateTime.UtcNow;
        operation.UpdateAt = DateTime.UtcNow;
        operation.UpdatedBy = organizationId;

        await context.SaveChangesAsync();
    }

    public async Task RejectByOrganizationAsync(
        Guid organizationId,
        Guid operationId,
        ProcessingOperationDecisionDto dto)
    {
        var operation = await context.ProcessingOperations
            .FirstOrDefaultAsync(x =>
                x.Id == operationId &&
                x.IsActive != false);

        if (operation is null)
            throw new KeyNotFoundException(
                "Processing operation not found.");

        if (operation.OrganizationId != organizationId)
            throw new InvalidOperationException(
                "You are not the organization assigned to this operation.");

        if (operation.Status != "PendingOrganizationApproval")
            throw new InvalidOperationException(
                "This processing operation is not awaiting organization approval.");

        if (string.IsNullOrWhiteSpace(dto.RejectionReason))
            throw new InvalidOperationException(
                "Rejection reason is required.");

        operation.Status = "RejectedByOrganization";
        operation.OrganizationRespondedAt = DateTime.UtcNow;
        operation.OrganizationRejectionReason = dto.RejectionReason.Trim();
        operation.UpdateAt = DateTime.UtcNow;
        operation.UpdatedBy = organizationId;

        await context.SaveChangesAsync();
    }

    public async Task ApproveByManagerAsync(
        Guid managerId,
        Guid operationId)
    {
        var operation = await context.ProcessingOperations
            .FirstOrDefaultAsync(x =>
                x.Id == operationId &&
                x.IsActive != false);

        if (operation is null)
            throw new KeyNotFoundException(
                "Processing operation not found.");

        if (operation.Status != "PendingManagerApproval")
            throw new InvalidOperationException(
                "This processing operation is not awaiting manager approval.");

        operation.Status = "Approved";
        operation.ApprovedByManagerId = managerId;
        operation.ManagerRespondedAt = DateTime.UtcNow;
        operation.UpdateAt = DateTime.UtcNow;
        operation.UpdatedBy = managerId;

        await context.SaveChangesAsync();
    }

    public async Task RejectByManagerAsync(
        Guid managerId,
        Guid operationId,
        ProcessingOperationDecisionDto dto)
    {
        var operation = await context.ProcessingOperations
            .FirstOrDefaultAsync(x =>
                x.Id == operationId &&
                x.IsActive != false);

        if (operation is null)
            throw new KeyNotFoundException(
                "Processing operation not found.");

        if (operation.Status != "PendingManagerApproval")
            throw new InvalidOperationException(
                "This processing operation is not awaiting manager approval.");

        if (string.IsNullOrWhiteSpace(dto.RejectionReason))
            throw new InvalidOperationException(
                "Rejection reason is required.");

        operation.Status = "RejectedByManager";
        operation.ManagerRespondedAt = DateTime.UtcNow;
        operation.ManagerRejectionReason = dto.RejectionReason.Trim();
        operation.UpdateAt = DateTime.UtcNow;
        operation.UpdatedBy = managerId;

        await context.SaveChangesAsync();
    }

    private static void ValidateOperationType(string operationType)
    {
        if (string.IsNullOrWhiteSpace(operationType))
            throw new InvalidOperationException(
                "Operation type is required.");

        if (operationType is not ("Recycling" or "Disposal"))
            throw new InvalidOperationException(
                "Operation type must be Recycling or Disposal.");
    }

    private static string BuildOperationCode(Guid id) =>
        $"PROC-{id.ToString("N")[..8].ToUpperInvariant()}";
}