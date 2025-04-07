using CPS.ComplexCases.API.Domain.Response;
using CPS.ComplexCases.Data.Services;
using CPS.ComplexCases.DDEI.Models.Dto;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.API.Services;

public class CaseEnrichmentService : ICaseEnrichmentService
{
  private readonly ICaseMetadataService _caseMetadataService;
  private readonly ILogger<CaseEnrichmentService> _logger;

  public CaseEnrichmentService(
      ICaseMetadataService caseMetadataService,
      ILogger<CaseEnrichmentService> logger)
  {
    _caseMetadataService = caseMetadataService;
    _logger = logger;
  }

  public async Task<IEnumerable<CaseWithMetadataResponse>> EnrichCasesWithMetadataAsync(IEnumerable<CaseDto> cases)
  {
    if (!cases.Any())
    {
      return cases.Select(MapCaseToResponse);
    }

    _logger.LogInformation("Enriching {CaseCount} cases with metadata", cases.Count());

    try
    {
      var casesResponse = cases.Select(MapCaseToResponse).ToList();

      var caseIds = cases.Select(c => c.CaseId).ToList();
      var metadataLookup = (await _caseMetadataService.GetCaseMetadataForCaseIdsAsync(caseIds))
          .ToDictionary(m => m.CaseId);

      foreach (var caseResponse in casesResponse)
      {
        if (metadataLookup.TryGetValue(caseResponse.CaseId, out var metadata))
        {
          caseResponse.EgressWorkspaceId = metadata.EgressWorkspaceId;
          caseResponse.NetappFolderPath = metadata.NetappFolderPath;
        }
      }

      return casesResponse;
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Failed to retrieve or apply metadata for cases");

      return cases.Select(MapCaseToResponse);
    }
  }

  private static CaseWithMetadataResponse MapCaseToResponse(CaseDto caseDto)
  {
    return new CaseWithMetadataResponse
    {
      CaseId = caseDto.CaseId,
      Urn = caseDto.Urn,
      OperationName = caseDto.OperationName,
      LeadDefendantName = caseDto.LeadDefendantName,
      RegistrationDate = caseDto.RegistrationDate
    };
  }
}