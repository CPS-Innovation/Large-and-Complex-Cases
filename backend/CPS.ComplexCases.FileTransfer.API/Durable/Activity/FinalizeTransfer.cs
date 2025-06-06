using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

public class FinalizeTransfer
{
    [Function(nameof(FinalizeTransfer))]
    public async Task Run([ActivityTrigger] FinalizeTransferPayload finalizeTransferPayload, [DurableClient] DurableTaskClient client)
    {
        var entityId = new EntityInstanceId(nameof(TransferEntityState), finalizeTransferPayload.TransferId.ToString());

        await client.Entities.SignalEntityAsync(entityId, nameof(TransferEntityState.Finalize), finalizeTransferPayload);
    }
}