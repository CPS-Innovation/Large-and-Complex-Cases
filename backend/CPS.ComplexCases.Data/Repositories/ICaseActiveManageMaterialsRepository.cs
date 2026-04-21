using CPS.ComplexCases.Data.Entities;

namespace CPS.ComplexCases.Data.Repositories;

public interface ICaseActiveManageMaterialsRepository
{
    Task InsertAsync(CaseActiveManageMaterialsOperation operation);
    Task DeleteAsync(Guid id);
    Task<IEnumerable<CaseActiveManageMaterialsOperation>> GetActiveOperationsForCaseAsync(int caseId);
    Task<bool> HasConflictingOperationAsync(int caseId, IEnumerable<string> sourcePaths, IEnumerable<string> destinationPaths);
}
