using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.API.Handlers;

public interface IUnhandledExceptionHandler
{
    public HttpResponseMessage HandleUnhandledException(
        ILogger logger,
        string logName,
        Exception ex,
        string additionalMessage = "");

    public StatusCodeResult HandleUnhandledExceptionActionResult(
        ILogger logger,
        string logName,
        Exception ex,
        string additionalMessage = "");
}