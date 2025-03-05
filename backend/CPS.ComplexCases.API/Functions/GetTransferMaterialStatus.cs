using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.API.Durable.Providers;
using Microsoft.DurableTask.Client;

namespace CPS.ComplexCases.API.Functions;

public class GetTransferMaterialStatus(ILogger<GetTransferMaterialStatus> logger,
  IOrchestrationProvider orchestrationProvider)
{
  private readonly ILogger<GetTransferMaterialStatus> _logger = logger;
  private readonly IOrchestrationProvider _orchestrationProvider = orchestrationProvider;

  [Function(nameof(GetTransferMaterialStatus))]
  public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "transfers/{transferId}")] HttpRequest req, string transferId, [DurableClient] DurableTaskClient client)
  {
    var response = await _orchestrationProvider.GetTransferMaterialStatusAsync(client, transferId);

    return new OkObjectResult(response);
  }
}