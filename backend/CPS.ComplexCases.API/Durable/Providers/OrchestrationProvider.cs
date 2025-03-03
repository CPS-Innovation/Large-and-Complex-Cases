
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask;
using CPS.ComplexCases.API.Domain;
using CPS.ComplexCases.API.Durable.Payloads;
using CPS.ComplexCases.API.Durable.Orchestration;
using System.Text.Json;

namespace CPS.ComplexCases.API.Durable.Providers;

public class OrchestrationProvider : IOrchestrationProvider
{
  public async Task<OrchestrationStatus> TransferMaterialAsync(DurableTaskClient client, TransferMaterialOrchestrationPayload payload)
  {
    var instanceId = TransferMaterialOrchestrator.GetKey(payload.OperationIdRoot);
    var existingInstance = await client.GetInstanceAsync(instanceId);

    if (existingInstance != null)
    {
      return MapRuntimeStatusToOrchestrationStatus(existingInstance.RuntimeStatus);
    }

    await client.ScheduleNewOrchestrationInstanceAsync(nameof(TransferMaterialOrchestrator), payload, options: new StartOrchestrationOptions
    {
      InstanceId = instanceId
    });

    return OrchestrationStatus.Accepted;
  }


  public async Task<IEnumerable<TransferMaterialStatus>> GetTransferMaterialStatusAsync(DurableTaskClient client, string operationIdRoot)
  {
    var query = new OrchestrationQuery
    {
      InstanceIdPrefix = $"[{operationIdRoot}]"
    };

    var instances = new List<OrchestrationMetadata>();
    await foreach (var instance in client.GetAllInstancesAsync(query))
    {
      instances.Add(instance);
    }

    return instances.Select(instance =>
    {
      var payload = instance.SerializedInput != null ? JsonSerializer.Deserialize<TransferMaterialOrchestrationPayload>(instance.SerializedInput) : null;
      return new TransferMaterialStatus
      {
        FilePath = payload?.Source ?? string.Empty,
        Status = MapRuntimeStatusToOrchestrationStatus(instance.RuntimeStatus)
      };
    });
  }

  private OrchestrationStatus MapRuntimeStatusToOrchestrationStatus(OrchestrationRuntimeStatus runtimeStatus)
  {
    return runtimeStatus switch
    {
      OrchestrationRuntimeStatus.Completed => OrchestrationStatus.Completed,
      OrchestrationRuntimeStatus.Failed => OrchestrationStatus.Failed,
      OrchestrationRuntimeStatus.Terminated => OrchestrationStatus.Failed,
      OrchestrationRuntimeStatus.Running => OrchestrationStatus.InProgress,
      OrchestrationRuntimeStatus.Pending => OrchestrationStatus.InProgress,
      OrchestrationRuntimeStatus.Suspended => OrchestrationStatus.InProgress,
      _ => OrchestrationStatus.Failed
    };
  }
}