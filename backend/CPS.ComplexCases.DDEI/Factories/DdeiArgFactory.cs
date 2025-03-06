using CPS.ComplexCases.DDEI.Models.Args;

namespace CPS.ComplexCases.DDEI.Factories;

public class DdeiArgFactory : IDdeiArgFactory
{
  public DdeiCaseIdArgDto CreateCaseArgFromUrnArg(DdeiUrnArgDto arg, int caseId)
  {
    return new DdeiCaseIdArgDto
    {
      CmsAuthValues = arg.CmsAuthValues,
      CorrelationId = arg.CorrelationId,
      CaseId = caseId
    };
  }

  public DdeiCaseIdArgDto CreateCaseArgFromDefendantArg(DdeiDefendantNameArgDto arg, int caseId)
  {
    return new DdeiCaseIdArgDto
    {
      CmsAuthValues = arg.CmsAuthValues,
      CorrelationId = arg.CorrelationId,
      CaseId = caseId
    };
  }

  public DdeiCaseIdArgDto CreateCaseArgFromOperationNameArg(DdeiOperationNameArgDto arg, int caseId)
  {
    return new DdeiCaseIdArgDto
    {
      CmsAuthValues = arg.CmsAuthValues,
      CorrelationId = arg.CorrelationId,
      CaseId = caseId
    };
  }
}