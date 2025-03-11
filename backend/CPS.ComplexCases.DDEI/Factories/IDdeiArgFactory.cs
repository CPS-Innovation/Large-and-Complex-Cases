using CPS.ComplexCases.DDEI.Models.Args;

namespace CPS.ComplexCases.DDEI.Factories;

public interface IDdeiArgFactory
{
  DdeiCaseIdArgDto CreateCaseArgFromUrnArg(DdeiUrnArgDto arg, int caseId);
  DdeiCaseIdArgDto CreateCaseArgFromDefendantArg(DdeiDefendantNameArgDto arg, int caseId);
  DdeiCaseIdArgDto CreateCaseArgFromOperationNameArg(DdeiOperationNameArgDto arg, int caseId);
  DdeiUrnArgDto CreateUrnArg(string cmsAuthValues, Guid correlationId, string urn);
  DdeiDefendantNameArgDto CreateDefendantArg(string cmsAuthValues, Guid correlationId, string leadDefendantLastName, string cmsAreaCode);
  DdeiOperationNameArgDto CreateOperationNameArg(string cmsAuthValues, Guid correlationId, string operationName, string cmsAreaCode);
}