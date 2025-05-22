
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.Data.Models.Requests;

namespace CPS.ComplexCases.API.Services;

public interface ICaseMetadataService
{
  Task CreateEgressConnectionAsync(CreateEgressConnectionDto createEgressConnectionDto);
  Task CreateNetAppConnectionAsync(CreateNetAppConnectionDto createNetAppConnectionDto);
  Task<IEnumerable<CaseMetadata>> GetCaseMetadataForCaseIdsAsync(IEnumerable<int> caseIds);
  Task<IEnumerable<CaseMetadata>> GetCaseMetadataForEgressWorkspaceIdsAsync(IEnumerable<string> egressWorkspaceIds);
  Task<CaseMetadata?> GetCaseMetadataForCaseIdAsync(int caseId);
  Task<IEnumerable<CaseMetadata>> GetCaseMetadataForNetAppFolderPathsAsync(IEnumerable<string> netAppFolderPaths);
}