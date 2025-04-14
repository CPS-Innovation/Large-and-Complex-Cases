
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.Data.Models.Requests;

namespace CPS.ComplexCases.Data.Services;

public interface ICaseMetadataService
{
  Task CreateEgressConnectionAsync(CreateEgressConnectionDto createEgressConnectionDto);
  Task<IEnumerable<CaseMetadata>> GetCaseMetadataForCaseIdsAsync(IEnumerable<int> caseIds);
  Task<IEnumerable<CaseMetadata>> GetCaseMetadataForEgressWorkspaceIdsAsync(IEnumerable<string> egressWorkspaceIds);
}