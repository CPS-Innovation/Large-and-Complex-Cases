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

  public DdeiUrnArgDto CreateUrnArg(string cmsAuthValues, Guid correlationId, string urn)
  {
    return new DdeiUrnArgDto
    {
      CmsAuthValues = cmsAuthValues,
      CorrelationId = correlationId,
      Urn = urn
    };
  }

  public DdeiDefendantNameArgDto CreateDefendantArg(string cmsAuthValues, Guid correlationId, string leadDefendantLastName, string cmsAreaCode)
  {
    return new DdeiDefendantNameArgDto
    {
      CmsAuthValues = cmsAuthValues,
      CorrelationId = correlationId,
      LastName = leadDefendantLastName,
      CmsAreaCode = cmsAreaCode
    };
  }

  public DdeiOperationNameArgDto CreateOperationNameArg(string cmsAuthValues, Guid correlationId, string operationName, string cmsAreaCode)
  {
    return new DdeiOperationNameArgDto
    {
      CmsAuthValues = cmsAuthValues,
      CorrelationId = correlationId,
      OperationName = operationName,
      CmsAreaCode = cmsAreaCode
    };
  }

  public DdeiBaseArgDto CreateBaseArg(string cmsAuthValues, Guid correlationId)
  {
    return new DdeiBaseArgDto
    {
      CmsAuthValues = cmsAuthValues,
      CorrelationId = correlationId
    };
  }

}