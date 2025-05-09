using CPS.ComplexCases.API.Domain.Response;
using CPS.ComplexCases.DDEI.Models.Dto;
using CPS.ComplexCases.Egress.Models.Dto;
using CPS.ComplexCases.NetApp.Models.Dto;

namespace CPS.ComplexCases.API.Services;

public interface ICaseEnrichmentService
{
  Task<IEnumerable<CaseWithMetadataResponse>> EnrichCasesWithMetadataAsync(IEnumerable<CaseDto> cases);
  Task<ListWorkspacesResponse> EnrichEgressWorkspacesWithMetadataAsync(ListWorkspacesDto workspaces);
  Task<ListNetAppObjectsResponse> EnrichNetAppFoldersWithMetadataAsync(ListNetAppObjectsDto folders);
}