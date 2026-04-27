using CPS.ComplexCases.Data.Entities;

namespace CPS.ComplexCases.Common.Services;

public interface ICaseActiveManageMaterialsService
{
    Task InsertOperationAsync(CaseActiveManageMaterialsOperation operation);
    Task DeleteOperationAsync(Guid id);
    Task<IEnumerable<CaseActiveManageMaterialsOperation>> GetActiveOperationsForCaseAsync(int caseId);
    Task<IEnumerable<CaseActiveManageMaterialsOperation>> GetAllActiveOperationsAsync();
    Task<bool> HasConflictingOperationAsync(int caseId, IEnumerable<string> sourcePaths, IEnumerable<string> destinationPaths);
}
