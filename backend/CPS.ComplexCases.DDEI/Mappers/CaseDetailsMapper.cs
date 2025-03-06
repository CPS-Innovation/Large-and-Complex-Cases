using CPS.ComplexCases.DDEI.Models.Dto;
using CPS.ComplexCases.DDEI.Models.Response;

namespace CPS.ComplexCases.DDEI.Mappers;

public class CaseDetailsMapper : ICaseDetailsMapper
{
  public CaseDto MapCaseDetails(DdeiCaseDetailsDto caseDetails)
  {
    return new CaseDto
    {
      CaseId = caseDetails.Summary.Id,
      Urn = caseDetails.Summary.Urn,
      LeadDefendantName = $"{caseDetails.Summary.LeadDefendantFirstNames} {caseDetails.Summary.LeadDefendantSurname}",
    };
  }
}