using CPS.ComplexCases.DDEI.Models.Dto;
using CPS.ComplexCases.DDEI.Models.Response;

namespace CPS.ComplexCases.DDEI.Mappers;

public interface ICaseDetailsMapper
{
  CaseDto MapCaseDetails(DdeiCaseSummaryDto caseDetails);
}