using System.Net;
using CPS.ComplexCases.DDEI.Models.Args;

namespace CPS.ComplexCases.DDEI.Factories;

public class DdeiRequestFactory(IMockSwitch mockSwitch) : IDdeiRequestFactory
{
  private const string CorrelationId = "Correlation-Id";
  private const string CmsAuthValues = "Cms-Auth-Values";
  private readonly IMockSwitch _mockSwitch = mockSwitch;

  public HttpRequestMessage CreateListCasesByUrnRequest(DdeiUrnArgDto arg) =>
    BuildRequest(HttpMethod.Get, $"api/urns/{Encode(arg.Urn)}/cases", arg);

  public HttpRequestMessage CreateListCasesByDefendantRequest(DdeiDefendantNameArgDto arg) =>
    BuildRequest(HttpMethod.Get, $"api/cases/find?defendant-name={Encode(arg.LastName)}&area-code={Encode(arg.CmsAreaCode)}", arg);

  public HttpRequestMessage CreateListCasesByOperationNameRequest(DdeiOperationNameArgDto arg) =>
    BuildRequest(HttpMethod.Get, $"api/cases/find?operation-name={Encode(arg.OperationName)}&area-code={Encode(arg.CmsAreaCode)}", arg);

  public HttpRequestMessage CreateGetCaseRequest(DdeiCaseIdArgDto arg) =>
    BuildRequest(HttpMethod.Get, $"api/cases/{arg.CaseId}/summary", arg);

  public HttpRequestMessage CreateUserFilteredDataRequest(DdeiBaseArgDto arg) =>
    BuildRequest(HttpMethod.Get, "api/user-filter-data", arg);

  public HttpRequestMessage CreateUserDataRequest(DdeiBaseArgDto arg) =>
    BuildRequest(HttpMethod.Get, "api/user-data", arg);

  public HttpRequestMessage CreateListUnitsRequest(DdeiBaseArgDto arg) =>
    BuildRequest(HttpMethod.Get, "api/units", arg);

  public HttpRequestMessage CreateGetCmsModernTokenRequest(DdeiBaseArgDto arg) =>
    BuildRequest(HttpMethod.Get, "api/user/cms-modern-token", arg);

  private HttpRequestMessage BuildRequest(HttpMethod httpMethod, string path, DdeiBaseArgDto arg)
  {
    var request = new HttpRequestMessage(httpMethod, _mockSwitch.BuildUri(arg.CmsAuthValues, path));
    request.Headers.Add(CmsAuthValues, arg.CmsAuthValues);
    request.Headers.Add(CorrelationId, arg.CorrelationId.ToString());
    return request;
  }

  private static string Encode(string param) => WebUtility.UrlEncode(param);

}