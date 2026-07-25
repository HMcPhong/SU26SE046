using System.Security.Claims;
using BLL.DTOs;
using BLL.Services.Interfaces.WarehouseOperations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capstone_API.Controllers;

[ApiController]
[Route("api/warehouse-operations")]
[Authorize(Roles = "WarehouseStaff,Manager")]
public class WarehouseOperationsController(IWarehouseOperationsService service) : ControllerBase
{
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard() => Ok(await service.GetDashboardAsync());

    [HttpGet("inbound-batches")]
    public async Task<IActionResult> InboundBatches() => Ok(await service.GetInboundBatchesAsync());

    [HttpGet("batches/{batchId:guid}")]
    public async Task<IActionResult> Batch(Guid batchId)
    {
        var batch = await service.GetBatchAsync(batchId);
        return batch is null ? NotFound() : Ok(batch);
    }

    [HttpPost("batches/{batchId:guid}/confirm-receipt")]
    public async Task<IActionResult> ConfirmReceipt(Guid batchId, ConfirmWarehouseReceiptDto dto)
    { await service.ConfirmReceiptAsync(CurrentUserId, batchId, dto); return NoContent(); }

    [HttpGet("batches/{batchId:guid}/locations")]
    public async Task<IActionResult> Locations(Guid batchId) => Ok(await service.GetLocationsAsync(batchId));

    [HttpPost("batches/{batchId:guid}/putaway")]
    public async Task<IActionResult> Putaway(Guid batchId, PutawayBatchDto dto)
    { await service.PutawayAsync(CurrentUserId, batchId, dto); return NoContent(); }

    [HttpGet("inventory")]
    public async Task<IActionResult> Inventory([FromQuery] string? search) =>
        Ok(await service.GetInventoryAsync(search));

    [HttpGet("transactions")]
    public async Task<IActionResult> Transactions([FromQuery] string? type) =>
        Ok(await service.GetTransactionsAsync(type));

    [HttpPost("inventory/{inventoryId:guid}/issue")]
    public async Task<IActionResult> Issue(Guid inventoryId, IssueInventoryDto dto)
    { await service.IssueAsync(CurrentUserId, inventoryId, dto); return NoContent(); }

    [HttpPost("inventory/{inventoryId:guid}/move")]
    public async Task<IActionResult> Move(Guid inventoryId, MoveInventoryDto dto)
    { await service.MoveAsync(CurrentUserId, inventoryId, dto); return NoContent(); }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
