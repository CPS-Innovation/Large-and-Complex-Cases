using CPS.ComplexCases.DDEI.Models.Args;

namespace CPS.ComplexCases.DDEI.Factories;

public interface IDdeiArgFactory
{
  DdeiCaseIdArgDto CreateCaseArgFromUrnArg(DdeiUrnArgDto arg, int caseId);
  DdeiCaseIdArgDto CreateCaseArgFromDefendantArg(DdeiDefendantNameArgDto arg, int caseId);
  DdeiCaseIdArgDto CreateCaseArgFromOperationNameArg(DdeiOperationNameArgDto arg, int caseId);
}