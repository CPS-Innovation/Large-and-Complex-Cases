using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Exceptions;
using CPS.ComplexCases.API.Validators;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace CPS.ComplexCases.API.Middleware;

public sealed partial class RequestValidationMiddleware(IAuthorizationValidator authorizationValidator) : IFunctionsWorkerMiddleware
{
  private readonly string[] _unauthenticatedRoutes = ["/api/status", "/api/tactical/login"];

  public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
  {
    var httpRequestData = await context.GetHttpRequestDataAsync() ?? throw new ArgumentNullException(nameof(context), "Context does not contains HttpRequestData");

    var correlationId = EstablishCorrelation(httpRequestData);
    var username = await AuthenticateOrThrow(httpRequestData);
    context.SetRequestContext(correlationId, username);

    await next(context);
  }

  private static Guid EstablishCorrelation(HttpRequestData httpRequestData)
  {
    if (httpRequestData.Headers.TryGetValues(HttpHeaderKeys.CorrelationId, out var correlationIds)
      && correlationIds.Any()
      && Guid.TryParse(correlationIds.First(), out var parsedCorrelationId))
    {
      return parsedCorrelationId;
    }

    return Guid.Empty;
  }

  private async Task<string?> AuthenticateOrThrow(HttpRequestData req)
  {
    var shouldAuthenticate = !_unauthenticatedRoutes.Any(req.Url.LocalPath.TrimEnd('/').Equals);
    if (!shouldAuthenticate)
    {
      return null;
    }

    if (!req.Headers.TryGetValues("Authorization", out var accessTokenValues) ||
        string.IsNullOrWhiteSpace(accessTokenValues.First()))
    {
      throw new CpsAuthenticationException();
    }

    var validateTokenResult = await authorizationValidator.ValidateTokenAsync(accessTokenValues.First(), "user_impersonation");

    if (validateTokenResult == null || validateTokenResult.Username == null)
    {
      throw new CpsAuthenticationException();
    }

    return validateTokenResult.IsValid ?
        validateTokenResult.Username :
        throw new CpsAuthenticationException();
  }
}