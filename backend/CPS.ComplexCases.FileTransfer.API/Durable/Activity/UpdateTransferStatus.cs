using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using CPS.ComplexCases.FileTransfer.API.Models.Domain;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

public class UpdateTransferStatus
{
    [Function(nameof(UpdateTransferStatus))]
    public async Task Run([ActivityTrigger] UpdateTransferStatusPayload updateStatusPayload, [DurableClient] DurableTaskClient client)
    {
        var entityId = new EntityInstanceId(nameof(TransferEntityState), updateStatusPayload.TransferId.ToString());

        await client.Entities.SignalEntityAsync(entityId, nameof(TransferEntityState.UpdateStatus), updateStatusPayload.Status);
    }
}