using BLL.DTOs;

namespace BLL.Services.Interfaces.ClassificationOperations;

public interface IClassificationOperationsService
{
    Task<IReadOnlyList<ClassificationBatchSummaryDto>> GetBatchesAsync();
    Task<ClassificationBatchDetailDto?> GetBatchAsync(Guid batchId);
    Task<ClassificationCatalogDto> GetCatalogAsync();
    Task StartBatchAsync(Guid staffId, Guid batchId);
    Task ConfirmReceiptAsync(Guid staffId, Guid batchId);
    Task<ClassificationItemDto> ClassifyItemAsync(Guid staffId, Guid batchId, ClassifyItemDto dto);
    Task CompleteBatchAsync(Guid staffId, Guid batchId);
    Task<IReadOnlyList<GroupedClassifiedBatchDto>> GetGroupedBatchesAsync(DateTime? date);
    Task<GroupedClassifiedBatchDetailDto?> GetGroupedBatchAsync(Guid groupedBatchId);
    Task SendGroupedBatchToWarehouseAsync(Guid staffId, Guid groupedBatchId);
}
