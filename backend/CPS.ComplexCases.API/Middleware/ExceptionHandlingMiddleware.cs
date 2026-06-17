using System.Net;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Exceptions;
using CPS.ComplexCases.DDEI.Exceptions;
using CPS.ComplexCases.NetApp.Exceptions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;

namespace CPS.ComplexCases.API.Middleware;

public class ExceptionHandlingMiddleware : IFunctionsWorkerMiddleware
{
  private readonly ILogger<ExceptionHandlingMiddleware> _logger;

  public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
  {
    _logger = logger;
  }

  public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
  {
    try
    {
      await next(context);
    }
    catch (Exception exception)
    {
      var statusCode = MapExceptionToStatusCode(exception);

      var httpRequestData = await context.GetHttpRequestDataAsync();

      if (httpRequestData != null)
      {
        var correlationId = Guid.NewGuid();
        try
        {
          correlationId = context.GetRequestContext().CorrelationId;
        }
        catch
        {
          _logger.LogTrace("Using fallback CorrelationId: {CorrelationId}", correlationId);
        }

        _logger.LogError(exception, "Unhandled exception. CorrelationId: {CorrelationId}", correlationId);

        var errorMessage = statusCode == HttpStatusCode.ServiceUnavailable
          ? "A downstream service is temporarily unavailable. Please retry shortly."
          : "An unexpected error occurred. Please contact support with the CorrelationId.";

        var response = httpRequestData.CreateResponse(statusCode);
        await response.WriteAsJsonAsync(new
        {
          ErrorMessage = errorMessage,
          CorrelationId = correlationId
        });

        var invocationResult = context.GetInvocationResult();
        var httpOutputBinding = GetHttpOutputBindingFromMultipleOutputBinding(context);

        if (httpOutputBinding is not null)
        {
          httpOutputBinding.Value = response;
        }
        else
        {
          invocationResult.Value = response;
        }
      }
      else
      {
        // If no HTTP request context exists, still log safely
        _logger.LogError(exception, "Unhandled exception outside HTTP context.");
      }
    }
  }

  // An open circuit surfaces as a BrokenCircuitException; map it to 503 so callers get a clear
  // "try again" signal rather than an unhandled 500.
  internal static HttpStatusCode MapExceptionToStatusCode(Exception exception) => exception switch
  {
    ArgumentNullException or BadRequestException => HttpStatusCode.BadRequest,
    CmsUnauthorizedException or CpsAuthenticationException or NetAppUnauthorizedException => HttpStatusCode.Unauthorized,
    MissingSecurityGroupException or NetAppAccessDeniedException => HttpStatusCode.Forbidden,
    BrokenCircuitException => HttpStatusCode.ServiceUnavailable,
    DdeiClientException ddeiException => ddeiException.StatusCode,
    _ => HttpStatusCode.InternalServerError,
  };

  private static OutputBindingData<HttpResponseData>? GetHttpOutputBindingFromMultipleOutputBinding(FunctionContext context)
  {
    // The output binding entry name will be "$return" only when the function return type is HttpResponseData
    return context.GetOutputBindings<HttpResponseData>()
        .FirstOrDefault(b => b.BindingType == "http" && b.Name != "$return");
  }
}

