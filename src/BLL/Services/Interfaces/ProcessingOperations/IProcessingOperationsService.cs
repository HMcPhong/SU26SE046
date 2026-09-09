namespace BLL.Services.Interfaces.ProcessingOperations;

using BLL.DTOs;

public interface IProcessingOperationsService
{
    Task<ProcessingOperationCreatedDto> CreateAsync(
        Guid userId,
        CreateProcessingOperationDto dto);

    Task ApproveByOrganizationAsync(
        Guid organizationId,
        Guid operationId);

    Task RejectByOrganizationAsync(
        Guid organizationId,
        Guid operationId,
        ProcessingOperationDecisionDto dto);

    Task ApproveByManagerAsync(
        Guid managerId,
        Guid operationId);

    Task RejectByManagerAsync(
        Guid managerId,
        Guid operationId,
        ProcessingOperationDecisionDto dto);
}