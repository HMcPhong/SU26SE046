using BLL.DTOs;

namespace BLL.Services.Interfaces.WarehouseOperations;

public interface IWarehouseOperationsService
{
    Task<WarehouseDashboardDto> GetDashboardAsync();
    Task<IReadOnlyList<WarehouseInboundBatchDto>> GetInboundBatchesAsync();
    Task<WarehouseInboundBatchDto?> GetBatchAsync(Guid batchId);
    Task ConfirmReceiptAsync(Guid staffId, Guid batchId, ConfirmWarehouseReceiptDto dto);
    Task<IReadOnlyList<StorageLocationDto>> GetLocationsAsync(Guid batchId);
    Task PutawayAsync(Guid staffId, Guid batchId, PutawayBatchDto dto);
    Task<IReadOnlyList<WarehouseInventoryDto>> GetInventoryAsync(string? search);
    Task<IReadOnlyList<WarehouseTransactionDto>> GetTransactionsAsync(string? type);
    Task IssueAsync(Guid staffId, Guid inventoryId, IssueInventoryDto dto);
    Task MoveAsync(Guid staffId, Guid inventoryId, MoveInventoryDto dto);
}
