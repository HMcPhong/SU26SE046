namespace BLL.DTOs;

public record CreateProcessingOperationDto(
    string OperationType,
    Guid WarehouseId,
    Guid OrganizationId,
    string? RequestNotes,
    List<CreateProcessingOperationInputDto> Inputs);

public record CreateProcessingOperationInputDto(
    Guid InventoryId,
    int RequestedQuantity,
    decimal RequestedWeight);

public record ProcessingOperationCreatedDto(
    Guid Id,
    string OperationCode,
    string OperationType,
    string Status);

public record ProcessingOperationDecisionDto(
    string? RejectionReason);