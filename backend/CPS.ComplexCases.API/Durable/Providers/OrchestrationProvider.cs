
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask;
using CPS.ComplexCases.API.Domain;
using CPS.ComplexCases.API.Durable.Payloads;
using CPS.ComplexCases.API.Durable.Orchestration;

namespace CPS.ComplexCases.API.Durable.Providers;

public class OrchestrationProvider : IOrchestrationProvider
{
  private static readonly OrchestrationRuntimeStatus[] InProgressStatuses =
  [
      OrchestrationRuntimeStatus.Running,
              OrchestrationRuntimeStatus.Pending,
              OrchestrationRuntimeStatus.Suspended
  ];

  private static readonly OrchestrationRuntimeStatus[] CompletedStatuses =
  [
      OrchestrationRuntimeStatus.Completed,
            OrchestrationRuntimeStatus.Failed,
            OrchestrationRuntimeStatus.Terminated
  ];

  public async Task<OrchestrationStatus> TransferMaterialAsync(DurableTaskClient client, TransferMaterialOrchestrationPayload payload)
  {
    var instanceId = TransferMaterialOrchestrator.GetKey(payload.WorkspaceId, payload.DocumentId);
    var existingInstance = await client.GetInstanceAsync(instanceId);

    if (existingInstance != null)
    {
      if (InProgressStatuses.Contains(existingInstance.RuntimeStatus))
      {
        return OrchestrationStatus.InProgress;
      }

      if (CompletedStatuses.Contains(existingInstance.RuntimeStatus))
      {
        return OrchestrationStatus.Completed;
      }
    }

    await client.ScheduleNewOrchestrationInstanceAsync(nameof(TransferMaterialOrchestrator), payload, options: new StartOrchestrationOptions
    {
      InstanceId = instanceId
    });

    return OrchestrationStatus.Accepted;
  }
}