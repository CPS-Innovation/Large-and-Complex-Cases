using CPS.ComplexCases.Data.Entities;

namespace CPS.ComplexCases.Data.Repositories
{
  public interface ICaseMetadataRepository
  {
    Task<CaseMetadata?> GetByCaseIdAsync(int caseId);
    Task<CaseMetadata> AddAsync(CaseMetadata metadata);
    Task<CaseMetadata?> UpdateAsync(CaseMetadata metadata);
    Task<IEnumerable<CaseMetadata>> GetByCaseIdsAsync(IEnumerable<int> caseIds);
    Task<IEnumerable<CaseMetadata>> GetByEgressWorkspaceIdsAsync(IEnumerable<string> egressWorkspaceIds);
    Task<IEnumerable<CaseMetadata>> GetByNetAppFolderPathsAsync(IEnumerable<string> netAppFolderPaths);
    Task<CaseMetadata?> GetByActiveTransferIdAsync(Guid activeTransferId);
  }
}