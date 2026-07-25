using System.Security.Claims;
using BLL.DTOs;
using BLL.Services.Interfaces.ClassificationOperations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capstone_API.Controllers;

[ApiController]
[Route("api/classification-operations")]
[Authorize(Roles = "ClassificationStaff")]
public class ClassificationOperationsController(IClassificationOperationsService service) : ControllerBase
{
    [HttpGet("batches")]
    public async Task<IActionResult> GetBatches() => Ok(await service.GetBatchesAsync());

    [HttpGet("batches/{batchId:guid}")]
    public async Task<IActionResult> GetBatch(Guid batchId)
    {
        var batch = await service.GetBatchAsync(batchId);
        return batch is null ? NotFound() : Ok(batch);
    }

    [HttpGet("catalog")]
    public async Task<IActionResult> GetCatalog() => Ok(await service.GetCatalogAsync());

    [HttpPost("batches/{batchId:guid}/start")]
    public async Task<IActionResult> Start(Guid batchId)
    { await service.StartBatchAsync(CurrentUserId, batchId); return NoContent(); }

    [HttpPost("batches/{batchId:guid}/confirm-receipt")]
    public async Task<IActionResult> ConfirmReceipt(Guid batchId)
    { await service.ConfirmReceiptAsync(CurrentUserId, batchId); return NoContent(); }

    [HttpPost("batches/{batchId:guid}/items")]
    public async Task<IActionResult> ClassifyItem(Guid batchId, ClassifyItemDto dto) =>
        Ok(await service.ClassifyItemAsync(CurrentUserId, batchId, dto));

    [HttpPost("batches/{batchId:guid}/complete")]
    public async Task<IActionResult> Complete(Guid batchId)
    { await service.CompleteBatchAsync(CurrentUserId, batchId); return NoContent(); }

    [HttpGet("grouped-batches")]
    public async Task<IActionResult> GetGroupedBatches([FromQuery] DateTime? date) =>
        Ok(await service.GetGroupedBatchesAsync(date));

    [HttpGet("grouped-batches/{groupedBatchId:guid}")]
    public async Task<IActionResult> GetGroupedBatch(Guid groupedBatchId)
    {
        var batch = await service.GetGroupedBatchAsync(groupedBatchId);
        return batch is null ? NotFound() : Ok(batch);
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
