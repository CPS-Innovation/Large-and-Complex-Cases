using System.Net;
using System.Text;
using CPS.ComplexCases.DDEI.Models.Args;

namespace CPS.ComplexCases.DDEI.Factories;

public class DdeiRequestFactory : IDdeiRequestFactory
{
  private const string CorrelationId = "Correlation-Id";
  private const string CmsAuthValues = "Cms-Auth-Values";

  public HttpRequestMessage CreateListCasesByUrnRequest(DdeiUrnArgDto arg)
  {
    var request = new HttpRequestMessage(HttpMethod.Get, $"api/urns/{Encode(arg.Urn)}/cases");
    AddAuthHeaders(request, arg);
    return request;
  }

  public HttpRequestMessage CreateListCasesByDefendantRequest(DdeiDefendantNameArgDto arg)
  {
    var url = $"api/cases/find?defendant-name={Encode(arg.LastName)}&area-code={Encode(arg.CmsAreaCode)}";

    var request = new HttpRequestMessage(HttpMethod.Get, url);

    AddAuthHeaders(request, arg);
    return request;
  }

  public HttpRequestMessage CreateListCasesByOperationNameRequest(DdeiOperationNameArgDto arg)
  {
    var url = $"api/cases/find?operation-name={Encode(arg.OperationName)}&area-code={Encode(arg.CmsAreaCode)}";

    var request = new HttpRequestMessage(HttpMethod.Get, url);

    AddAuthHeaders(request, arg);
    return request;
  }

  public HttpRequestMessage CreateGetCaseRequest(DdeiCaseIdArgDto arg)
  {
    var request = new HttpRequestMessage(HttpMethod.Get, $"api/cases/{arg.CaseId}/summary");
    AddAuthHeaders(request, arg);
    return request;
  }

  public HttpRequestMessage CreateUserFilteredDataRequest(DdeiBaseArgDto arg)
  {
    var request = new HttpRequestMessage(HttpMethod.Get, "api/user-filter-data");
    AddAuthHeaders(request, arg);
    return request;
  }

  public HttpRequestMessage CreateUserDataRequest(DdeiBaseArgDto arg)
  {
    var request = new HttpRequestMessage(HttpMethod.Get, "api/user-data");
    AddAuthHeaders(request, arg);
    return request;
  }
  public HttpRequestMessage CreateListUnitsRequest(DdeiBaseArgDto arg)
  {
    var request = new HttpRequestMessage(HttpMethod.Get, "api/units");
    AddAuthHeaders(request, arg);
    return request;
  }

  private void AddAuthHeaders(HttpRequestMessage request, DdeiBaseArgDto arg)
  {
    request.Headers.Add(CmsAuthValues, arg.CmsAuthValues);
    request.Headers.Add(CorrelationId, arg.CorrelationId.ToString());
  }

  private string Encode(string param) => WebUtility.UrlEncode(param);

}