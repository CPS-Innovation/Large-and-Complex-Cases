using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Mvc;
using CPS.ComplexCases.API.Durable.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.DurableTask.Client;
using CPS.ComplexCases.API.Durable.Payloads;
using CPS.ComplexCases.API.Handlers;
using CPS.ComplexCases.API.Domain;

namespace CPS.ComplexCases.API.Functions;

public class TransferMaterial(ILogger<TransferMaterial> logger,
  IOrchestrationProvider orchestrationProvider,
  IUnhandledExceptionHandler exceptionHandler)
{
  private readonly ILogger<TransferMaterial> _logger = logger;
  private readonly IOrchestrationProvider _orchestrationProvider = orchestrationProvider;
  private readonly IUnhandledExceptionHandler _unhandledExceptionHandler = exceptionHandler;

  [Function(nameof(TransferMaterial))]
  public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "workspaces/{workspaceId}/files/{fileId}")] HttpRequest req, [DurableClient] DurableTaskClient client,
    string workspaceId, string fileId)
  {
    try
    {
      var payload = new TransferMaterialOrchestrationPayload(workspaceId, fileId);

      var result = await _orchestrationProvider.TransferMaterialAsync(client, payload);


      return result switch
      {
        OrchestrationStatus.Accepted or OrchestrationStatus.Completed => new ObjectResult(new TransferResponse(workspaceId, fileId))
        {
          StatusCode = StatusCodes.Status202Accepted
        },
        OrchestrationStatus.InProgress => new StatusCodeResult(StatusCodes.Status423Locked),
        _ => new StatusCodeResult(StatusCodes.Status500InternalServerError),
      };
    }
    catch (Exception ex)
    {
      return _unhandledExceptionHandler.HandleUnhandledExceptionActionResult(_logger, nameof(TransferMaterial), ex);
    }
  }
}