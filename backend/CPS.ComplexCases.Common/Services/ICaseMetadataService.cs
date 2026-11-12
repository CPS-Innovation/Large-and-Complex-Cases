
using CPS.ComplexCases.Common.Models.Results;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.Data.Models.Requests;

namespace CPS.ComplexCases.Common.Services;

public interface ICaseMetadataService
{
    Task<CreateConnectionResult> CreateEgressConnectionAsync(CreateEgressConnectionDto createEgressConnectionDto);
    Task<CreateConnectionResult> CreateNetAppConnectionAsync(CreateNetAppConnectionDto createNetAppConnectionDto);
    Task<IEnumerable<CaseMetadata>> GetCaseMetadataForCaseIdsAsync(IEnumerable<int> caseIds);
    Task<IEnumerable<CaseMetadata>> GetCaseMetadataForEgressWorkspaceIdsAsync(IEnumerable<string> egressWorkspaceIds);
    Task<CaseMetadata?> GetCaseMetadataForCaseIdAsync(int caseId);
    Task<IEnumerable<CaseMetadata>> GetCaseMetadataForNetAppFolderPathsAsync(IEnumerable<string> netAppFolderPaths);
    Task UpdateActiveTransferIdAsync(int caseId, Guid? activeTransferId);
    Task<bool> ClearActiveTransferIdAsync(Guid transferId);
    Task<ClearFolderPathResult> ClearNetAppFolderPathAsync(int caseId);
    Task<ClearFolderPathResult> ClearEgressConnectionAsync(int caseId);
}
