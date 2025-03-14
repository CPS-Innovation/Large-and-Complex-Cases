using CPS.ComplexCases.DDEI.Models.Args;

namespace CPS.ComplexCases.DDEI.Factories;

public interface IDdeiRequestFactory
{
  HttpRequestMessage CreateListCasesByUrnRequest(DdeiUrnArgDto arg);
  HttpRequestMessage CreateListCasesByDefendantRequest(DdeiDefendantNameArgDto arg);
  HttpRequestMessage CreateListCasesByOperationNameRequest(DdeiOperationNameArgDto arg);
  HttpRequestMessage CreateGetCaseRequest(DdeiCaseIdArgDto arg);
  HttpRequestMessage CreateUserFilteredDataRequest(DdeiBaseArgDto arg);
}