using CPS.ComplexCases.Data.Entities;

namespace CPS.ComplexCases.Data.Repositories
{
  public interface ICaseMetadataRepository
  {
    Task<CaseMetadata?> GetByCaseIdAsync(int caseId);
    Task<CaseMetadata> AddAsync(CaseMetadata metadata);
    Task<CaseMetadata?> UpdateAsync(CaseMetadata metadata);
    Task<IEnumerable<CaseMetadata>> GetByCaseIdsAsync(IEnumerable<int> caseIds);
  }
}