using CPS.ComplexCases.API.Domain.Response;
using CPS.ComplexCases.DDEI.Models.Dto;

namespace CPS.ComplexCases.API.Services;
public interface ICaseEnrichmentService
{
  Task<IEnumerable<CaseWithMetadataResponse>> EnrichCasesWithMetadataAsync(IEnumerable<CaseDto> cases);
}