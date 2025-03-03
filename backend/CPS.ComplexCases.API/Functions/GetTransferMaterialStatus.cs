using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.API.Handlers;
using CPS.ComplexCases.API.Durable.Providers;
using Microsoft.DurableTask.Client;

namespace CPS.ComplexCases.API.Functions;

public class GetTransferMaterialStatus(ILogger<GetTransferMaterialStatus> logger,
  IOrchestrationProvider orchestrationProvider,
  IInitializationHandler initializationHandler,
  IUnhandledExceptionHandler unhandledExceptionHandler)
{
  private readonly ILogger<GetTransferMaterialStatus> _logger = logger;
  private readonly IOrchestrationProvider _orchestrationProvider = orchestrationProvider;
  private readonly IInitializationHandler _initializationHandler = initializationHandler;
  private readonly IUnhandledExceptionHandler _unhandledExceptionHandler = unhandledExceptionHandler;

  [Function(nameof(GetTransferMaterialStatus))]
  public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "transfers/{transferId}")] HttpRequest req, string transferId, [DurableClient] DurableTaskClient client)
  {
    try
    {
      var validateTokenResult = await _initializationHandler.Initialize(req);

      if (!validateTokenResult.IsValid || string.IsNullOrEmpty(validateTokenResult.Username))
      {
        return new UnauthorizedResult();
      }

      var response = await _orchestrationProvider.GetTransferMaterialStatusAsync(client, transferId);

      return new OkObjectResult(response);
    }
    catch (Exception ex)
    {
      return _unhandledExceptionHandler.HandleUnhandledExceptionActionResult(_logger, nameof(GetTransferMaterialStatus), ex);
    }
  }
}