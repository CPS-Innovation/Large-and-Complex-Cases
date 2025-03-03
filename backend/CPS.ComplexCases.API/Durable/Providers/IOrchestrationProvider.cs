
using Microsoft.DurableTask.Client;
using CPS.ComplexCases.API.Durable.Payloads;
using CPS.ComplexCases.API.Domain;

namespace CPS.ComplexCases.API.Durable.Providers;

public interface IOrchestrationProvider
{
  Task<OrchestrationStatus> TransferMaterialAsync(DurableTaskClient client, TransferMaterialOrchestrationPayload payload);
  Task<IEnumerable<TransferMaterialStatus>> GetTransferMaterialStatusAsync(DurableTaskClient client, string operationIdRoot);
}
