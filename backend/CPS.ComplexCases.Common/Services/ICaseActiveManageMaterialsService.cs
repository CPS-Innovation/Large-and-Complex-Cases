using CPS.ComplexCases.Data.Entities;

namespace CPS.ComplexCases.Common.Services;

public interface ICaseActiveManageMaterialsService
{
    Task DeleteOperationAsync(Guid id);
    Task<IEnumerable<CaseActiveManageMaterialsOperation>> GetActiveOperationsForCaseAsync(int caseId);
    Task<IEnumerable<CaseActiveManageMaterialsOperation>> GetAllActiveOperationsAsync();
    Task<bool> CheckConflictAndInsertAsync(CaseActiveManageMaterialsOperation operation, IEnumerable<string> sourcePaths, IEnumerable<string> destinationPaths);
}
