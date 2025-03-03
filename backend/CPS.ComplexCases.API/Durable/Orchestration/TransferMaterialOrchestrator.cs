using CPS.ComplexCases.API.Durable.Activity;
using CPS.ComplexCases.API.Durable.Payloads;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;

namespace CPS.ComplexCases.API.Durable.Orchestration;

public class TransferMaterialOrchestrator
{
  public static string GetKey(Guid operationIdRoot)
  {
    return $"[{operationIdRoot}]-{Guid.NewGuid()}";
  }

  [Function(nameof(TransferMaterialOrchestrator))]
  public async Task Run([OrchestrationTrigger] TaskOrchestrationContext context)
  {
    var payload = context.GetInput<TransferMaterialOrchestrationPayload>() ?? throw new ArgumentException("Orchestration payload cannot be null.", nameof(context));

    await context.CallActivityAsync(nameof(InitiateTransferMaterial), payload);
  }
}