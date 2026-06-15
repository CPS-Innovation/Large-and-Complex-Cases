using CPS.ComplexCases.FileTransfer.API.Durable.Helpers;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging.Abstractions;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

public class FinalizeTransfer
{
    [Function(nameof(FinalizeTransfer))]
    public async Task Run([ActivityTrigger] FinalizeTransferPayload finalizeTransferPayload, [DurableClient] DurableTaskClient client, CancellationToken cancellationToken = default)
    {
        var entityId = new EntityInstanceId(nameof(TransferEntityState), finalizeTransferPayload.TransferId.ToString());

        await DurableEntityRetry.ExecuteAsync(
            nameof(FinalizeTransfer),
            () => client.Entities.SignalEntityAsync(entityId, nameof(TransferEntityState.FinalizeTransfer), cancellationToken),
            NullLogger.Instance,
            cancellationToken);
    }
}