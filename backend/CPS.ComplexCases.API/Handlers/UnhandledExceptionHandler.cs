using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.API.Exceptions;

namespace CPS.ComplexCases.API.Handlers;

public class UnhandledExceptionHandler : IUnhandledExceptionHandler
{
  public HttpResponseMessage HandleUnhandledException(
      ILogger logger,
      string logName,
      Exception ex,
      string additionalMessage = "")
  {
    var actionResponse = HandleUnhandledExceptionActionResult(logger, logName, ex, additionalMessage);

    return new HttpResponseMessage()
    {
      StatusCode = (HttpStatusCode)actionResponse.StatusCode
    };
  }

  public StatusCodeResult HandleUnhandledExceptionActionResult(
      ILogger logger,
      string logName,
      Exception ex,
      string additionalMessage = "")
  {
    logger.LogError(ex, $"{logName} failed. {additionalMessage}");

    var statusCode = ex switch
    {
      ArgumentNullException _ => StatusCodes.Status400BadRequest,
      CpsAuthenticationException _ => StatusCodes.Status407ProxyAuthenticationRequired,
      _ => StatusCodes.Status500InternalServerError,
    };

    return new StatusCodeResult(statusCode);
  }
}