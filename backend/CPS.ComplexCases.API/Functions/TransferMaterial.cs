using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Mvc;
using CPS.ComplexCases.API.Durable.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.DurableTask.Client;
using CPS.ComplexCases.API.Durable.Payloads;
using CPS.ComplexCases.API.Domain;
using CPS.ComplexCases.API.Validators;

namespace CPS.ComplexCases.API.Functions;

public class TransferMaterial(ILogger<TransferMaterial> logger,
  IOrchestrationProvider orchestrationProvider)
{
  private readonly ILogger<TransferMaterial> _logger = logger;
  private readonly IOrchestrationProvider _orchestrationProvider = orchestrationProvider;

  [Function(nameof(TransferMaterial))]
  public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "transfers")] HttpRequest req, [DurableClient] DurableTaskClient client)
  {
    var transferRequest = await ValidatorHelper.GetJsonBody<TransferMaterialDto, TransferMaterialValidator>(req);

    if (!transferRequest.IsValid)
    {
      return new BadRequestObjectResult(transferRequest.ValidationErrors);
    }

    var filePaths = transferRequest.Value.FilePaths;
    var operationIdRoot = Guid.NewGuid();

    foreach (var filePath in filePaths)
    {
      var payload = new TransferMaterialOrchestrationPayload(operationIdRoot, filePath.Source, filePath.Destination);
      await _orchestrationProvider.TransferMaterialAsync(client, payload);
    }

    return new ObjectResult(new TransferResponse(operationIdRoot));

  }
}