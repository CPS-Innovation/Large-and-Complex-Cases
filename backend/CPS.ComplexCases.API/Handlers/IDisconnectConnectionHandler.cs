using CPS.ComplexCases.API.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace CPS.ComplexCases.API.Handlers;

public interface IDisconnectConnectionHandler
{
    Task<IActionResult> RunAsync(HttpRequest req, FunctionContext functionContext, StorageConnectionType connectionType);
}