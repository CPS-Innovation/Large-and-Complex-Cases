
using System.Net;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Exceptions;
using CPS.ComplexCases.API.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

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
          correlationId = context.GetRequestContext().CorrelationId;
        }
        catch
        {
        }

        _logger.LogMethodError(correlationId, httpRequestData.Url.ToString(), message, exception);

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