
using CPS.ComplexCases.API.Exceptions;
using CPS.ComplexCases.API.Extensions;
using CPS.ComplexCases.API.Validators;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights;
using CPS.ComplexCases.API.Constants;

namespace CPS.ComplexCases.API.Middleware;

public sealed partial class RequestValidationMiddleware(
  IAuthorizationValidator authorizationValidator,
  TelemetryClient telemetryClient) : IFunctionsWorkerMiddleware
{
  private readonly string[] _unauthenticatedRoutes = ["/api/status"];

  public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
  {
    var requestTelemetry = new RequestTelemetry();
    requestTelemetry.Start();
    var requestData = await context.GetHttpRequestDataAsync();

    var correlationId = Guid.NewGuid();

    if (requestData != null && !_unauthenticatedRoutes.Any(requestData.Url.LocalPath.TrimEnd('/').Equals))
    {
      correlationId = requestData.EstablishCorrelation();

      var username = await AuthenticateRequest(requestData, correlationId);
      requestTelemetry.Properties[TelemetryConstants.UserCustomDimensionName] = username;
    }

    await next(context);

    requestTelemetry.Context.Cloud.RoleName = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
    requestTelemetry.Context.Operation.Name = context.FunctionDefinition.Name;
    requestTelemetry.Name = context.FunctionDefinition.Name;
    requestTelemetry.ResponseCode = context.GetHttpResponseData()?.StatusCode.ToString() ?? string.Empty;
    requestTelemetry.Success = true;
    requestTelemetry.Url = requestData?.Url;
    requestTelemetry.Stop();

    telemetryClient.TrackRequest(requestTelemetry);

    context.GetHttpResponseData()?.Headers.Add(HttpHeaderKeys.CorrelationId, correlationId.ToString());
  }
  private async Task<string> AuthenticateRequest(HttpRequestData req, Guid correlationId)
  {
    if (!req.Headers.TryGetValues("Authorization", out var accessTokenValues) ||
        string.IsNullOrWhiteSpace(accessTokenValues.First()))
    {
      throw new CpsAuthenticationException();
    }

    var validateTokenResult = await authorizationValidator.ValidateTokenAsync(accessTokenValues.First(), correlationId, "user_impersonation");

    if (validateTokenResult == null || validateTokenResult.Username == null)
    {
      throw new CpsAuthenticationException();
    }

    return validateTokenResult.IsValid ?
        validateTokenResult.Username :
        throw new CpsAuthenticationException();
  }
}