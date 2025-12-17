using System.Net;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Exceptions;
using CPS.ComplexCases.API.Validators;
using CPS.ComplexCases.Common.Helpers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace CPS.ComplexCases.API.Middleware;

public sealed partial class RequestValidationMiddleware(IAuthorizationValidator authorizationValidator) : IFunctionsWorkerMiddleware
{
  private readonly string[] _unauthenticatedRoutes = ["/api/status", "/api/tactical/login", "/api/swagger/ui", "/api/swagger.json", "/api/v1/init"];



  public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
  {
    var httpRequestData = await context.GetHttpRequestDataAsync() ?? throw new ArgumentNullException(nameof(context), "Context does not contains HttpRequestData");

    // Block requests to certain routes in production
    if (RouteBlockerHelper.ShouldBlockRoute(httpRequestData.Url.AbsolutePath))
    {
      var response = httpRequestData.CreateResponse(HttpStatusCode.NotFound);
      await response.WriteStringAsync("Not Found");
      context.GetInvocationResult().Value = response;
      return;
    }

    var correlationId = EstablishCorrelation(httpRequestData);
    var cmsAuthValues = EstablishCmsAuthValues(httpRequestData);
    var (isAuthenticated, username, token) = await Authenticate(httpRequestData);

    context.SetRequestContext(correlationId, cmsAuthValues, username, token);

    if (!isAuthenticated && !_unauthenticatedRoutes.Contains(httpRequestData.Url.AbsolutePath))
    {
      throw new CpsAuthenticationException();
    }

    await next(context);
  }

  private static Guid EstablishCorrelation(HttpRequestData httpRequestData)
  {
    if (httpRequestData.Headers.TryGetValues(Common.Constants.HttpHeaderKeys.CorrelationId, out var correlationIds)
      && correlationIds.Any()
      && Guid.TryParse(correlationIds.First(), out var parsedCorrelationId))
    {
      return parsedCorrelationId;
    }

    return Guid.Empty;
  }

  private static string? EstablishCmsAuthValues(HttpRequestData httpRequestData)
  {
    var cmsAuthValues = httpRequestData.Cookies.FirstOrDefault(cookie => cookie.Name == HttpHeaderKeys.CmsAuthValues);
    return cmsAuthValues?.Value;
  }

  private async Task<(bool, string?, string?)> Authenticate(HttpRequestData req)
  {
    try
    {
      if (!req.Headers.TryGetValues(HttpHeaderKeys.Authorization, out var accessTokenValues) ||
          string.IsNullOrWhiteSpace(accessTokenValues.First()))
      {
        return (false, null, null);
      }

      var validateTokenResult = await authorizationValidator.ValidateTokenAsync(accessTokenValues.First(), "user_impersonation");

      if (validateTokenResult == null || validateTokenResult.Username == null)
      {
        return (false, null, null);
      }

      return validateTokenResult.IsValid
          ? (true, validateTokenResult.Username, validateTokenResult.Token)
          : (false, null, null);
    }
    catch (Exception)
    {
      throw new CpsAuthenticationException();
    }
  }
}