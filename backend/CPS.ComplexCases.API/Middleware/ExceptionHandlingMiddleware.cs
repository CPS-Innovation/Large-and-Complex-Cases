
using CPS.ComplexCases.API.Exceptions;
using CPS.ComplexCases.API.Extensions;
using Microsoft.ApplicationInsights;
using CPS.ComplexCases.API.Constants;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System.Net;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace CPS.ComplexCases.API.Middleware;

public class ExceptionHandlingMiddleware : IFunctionsWorkerMiddleware
{
  private readonly ILogger<ExceptionHandlingMiddleware> _logger;
  private readonly TelemetryClient _telemetryClient;

  public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger, TelemetryClient telemetryClient)
  {
    _logger = logger;
    _telemetryClient = telemetryClient;
  }

  public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
  {
    var requestTelemetry = new RequestTelemetry();
    requestTelemetry.Start();

    try
    {
      await next(context);
    }
    catch (Exception exception)
    {
      var statusCode = exception switch
      {
        BadRequestException _ => HttpStatusCode.BadRequest,
        ArgumentNullException or BadRequestException _ => HttpStatusCode.BadRequest,
        CpsAuthenticationException _ => HttpStatusCode.ProxyAuthenticationRequired,
        _ => HttpStatusCode.InternalServerError,
      };

      var message = string.Empty;

      var httpRequestData = await context.GetHttpRequestDataAsync();

      if (httpRequestData != null)
      {
        var correlationId = Guid.NewGuid();
        try
        {
          correlationId = httpRequestData.EstablishCorrelation();
        }
        catch
        {
        }

        _logger.LogMethodError(correlationId, httpRequestData.Url.ToString(), message, exception);
        requestTelemetry.Properties[TelemetryConstants.CorrelationIdCustomDimensionName] = correlationId.ToString();

        var newHttpResponse = httpRequestData.CreateResponse(statusCode);

        await newHttpResponse.WriteAsJsonAsync(new { ErrorMessage = exception.ToStringFullResponse(), CorrelationId = correlationId });

        var invocationResult = context.GetInvocationResult();

        var httpOutputBindingFromMultipleOutputBindings = GetHttpOutputBindingFromMultipleOutputBinding(context);
        if (httpOutputBindingFromMultipleOutputBindings is not null)
        {
          httpOutputBindingFromMultipleOutputBindings.Value = newHttpResponse;
        }
        else
        {
          invocationResult.Value = newHttpResponse;
        }

        requestTelemetry.Context.Cloud.RoleName = Environment.GetEnvironmentVariable("WEBSITE_HOSTNAME");
        requestTelemetry.Context.Operation.Name = context.FunctionDefinition.Name;
        requestTelemetry.Name = context.FunctionDefinition.Name;
#pragma warning disable CS0618 // Type or member is obsolete
        requestTelemetry.HttpMethod = httpRequestData.Method;
#pragma warning restore CS0618 // Type or member is obsolete
        requestTelemetry.ResponseCode = ((int)statusCode).ToString();
        requestTelemetry.Success = false;
        requestTelemetry.Url = httpRequestData.Url;
        requestTelemetry.Properties[TelemetryConstants.ErrorMessageCustomDimensionName] = exception.ToStringFullResponse();
        requestTelemetry.Stop();

        _telemetryClient.TrackRequest(requestTelemetry);
      }
    }
  }

  private static OutputBindingData<HttpResponseData> GetHttpOutputBindingFromMultipleOutputBinding(FunctionContext context)
  {
    // The output binding entry name will be "$return" only when the function return type is HttpResponseData
    var httpOutputBinding = context.GetOutputBindings<HttpResponseData>()
        .FirstOrDefault(b => b.BindingType == "http" && b.Name != "$return");

    return httpOutputBinding ?? throw new InvalidOperationException("HttpOutputBinding is null");
  }
}