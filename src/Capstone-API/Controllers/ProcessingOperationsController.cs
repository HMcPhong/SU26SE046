using System.Security.Claims;
using BLL.DTOs;
using BLL.Services.Interfaces.ProcessingOperations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capstone_API.Controllers;

[ApiController]
[Route("api/processing-operations")]
[Authorize]
public class ProcessingOperationsController(
    IProcessingOperationsService service) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> Create(CreateProcessingOperationDto dto) =>Ok(await service.CreateAsync(CurrentUserId, dto));

    [HttpPost("{id:guid}/organization/approve")]
    [Authorize(Roles = "RecyclingOrganization,DisposalOrganization")]
    public async Task<IActionResult> ApproveByOrganization(Guid id)
    {
        await service.ApproveByOrganizationAsync(CurrentUserId,id);
        return Ok();
    }

    [HttpPost("{id:guid}/organization/reject")]
    [Authorize(Roles = "RecyclingOrganization,DisposalOrganization")]
    public async Task<IActionResult> RejectByOrganization(Guid id,ProcessingOperationDecisionDto dto)
    {
        await service.RejectByOrganizationAsync(CurrentUserId,id,dto);
        return Ok();
    }

    [HttpPost("{id:guid}/manager/approve")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> ApproveByManager(Guid id)
    {
        await service.ApproveByManagerAsync(CurrentUserId,id);
        return Ok();
    }

    [HttpPost("{id:guid}/manager/reject")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> RejectByManager(Guid id,ProcessingOperationDecisionDto dto)
    {
        await service.RejectByManagerAsync(CurrentUserId,id,dto);
        return Ok();
    }

    private Guid CurrentUserId =>Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}