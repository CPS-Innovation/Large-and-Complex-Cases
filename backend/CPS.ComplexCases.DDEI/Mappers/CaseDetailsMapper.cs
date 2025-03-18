using CPS.ComplexCases.DDEI.Models.Dto;
using CPS.ComplexCases.DDEI.Models.Response;

namespace CPS.ComplexCases.DDEI.Mappers;

public class CaseDetailsMapper : ICaseDetailsMapper
{
  public CaseDto MapCaseDetails(DdeiCaseSummaryDto caseDetails)
  {
    return new CaseDto
    {
      CaseId = caseDetails.Id,
      Urn = caseDetails.Urn,
      LeadDefendantName = $"{caseDetails.LeadDefendantFirstNames} {caseDetails.LeadDefendantSurname}",
      OperationName = caseDetails.Operation,
      RegistrationDate = caseDetails.RegistrationDate
    };
  }
}