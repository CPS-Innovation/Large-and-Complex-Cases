using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.Common.Services;

public class CaseActiveManageMaterialsService(
    ICaseActiveManageMaterialsRepository repository,
    ILogger<CaseActiveManageMaterialsService> logger) : ICaseActiveManageMaterialsService
{
    private readonly ICaseActiveManageMaterialsRepository _repository = repository;
    private readonly ILogger<CaseActiveManageMaterialsService> _logger = logger;

    public async Task InsertOperationAsync(CaseActiveManageMaterialsOperation operation)
    {
        _logger.LogInformation("Inserting manage materials operation {OperationId} of type {OperationType} for case {CaseId}",
            operation.Id, operation.OperationType, operation.CaseId);
        await _repository.InsertAsync(operation);
    }

    public async Task DeleteOperationAsync(Guid id)
    {
        _logger.LogInformation("Removing manage materials operation {OperationId}", id);
        try
        {
            await _repository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing manage materials operation {OperationId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<CaseActiveManageMaterialsOperation>> GetActiveOperationsForCaseAsync(int caseId)
    {
        return await _repository.GetActiveOperationsForCaseAsync(caseId);
    }

    public async Task<IEnumerable<CaseActiveManageMaterialsOperation>> GetAllActiveOperationsAsync()
    {
        return await _repository.GetAllActiveOperationsAsync();
    }

    public async Task<bool> HasConflictingOperationAsync(int caseId, IEnumerable<string> sourcePaths, IEnumerable<string> destinationPaths)
    {
        return await _repository.HasConflictingOperationAsync(caseId, sourcePaths, destinationPaths);
    }
}
