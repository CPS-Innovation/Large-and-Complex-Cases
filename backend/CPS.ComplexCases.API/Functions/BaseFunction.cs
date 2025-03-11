
using CPS.ComplexCases.API.Constants;
using Microsoft.AspNetCore.Http;

namespace CPS.ComplexCases.API.Functions;

public class BaseFunction()
{
  protected static Guid EstablishCorrelation(HttpRequest req) =>
      req.Headers.TryGetValue(HttpHeaderKeys.CorrelationId, out var correlationId) &&
      Guid.TryParse(correlationId, out var parsedCorrelationId) ?
          parsedCorrelationId :
          Guid.NewGuid();

  protected static string EstablishCmsAuthValues(HttpRequest req)
  {
    req.Cookies.TryGetValue(HttpHeaderKeys.CmsAuthValues, out var cmsAuthValues);
    return cmsAuthValues ?? throw new ArgumentNullException(HttpHeaderKeys.CmsAuthValues);
  }
}