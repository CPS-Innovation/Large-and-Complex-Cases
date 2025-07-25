using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

public class InitializeTransfer
{
    [Function(nameof(InitializeTransfer))]
    public async Task Run([ActivityTrigger] TransferEntity initialEntity, [DurableClient] DurableTaskClient client, CancellationToken cancellationToken = default)
    {
        var entityId = new EntityInstanceId(nameof(TransferEntityState), initialEntity.Id.ToString());

        await client.Entities.SignalEntityAsync(entityId, nameof(TransferEntityState.Initialize), initialEntity, null, cancellationToken);
    }
}