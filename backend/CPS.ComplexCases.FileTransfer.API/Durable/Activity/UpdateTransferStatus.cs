using CPS.ComplexCases.FileTransfer.API.Durable.Helpers;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging.Abstractions;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

public class UpdateTransferStatus
{
    [Function(nameof(UpdateTransferStatus))]
    public async Task Run([ActivityTrigger] UpdateTransferStatusPayload updateStatusPayload, [DurableClient] DurableTaskClient client, CancellationToken cancellationToken = default)
    {
        var entityId = new EntityInstanceId(nameof(TransferEntityState), updateStatusPayload.TransferId.ToString());

        await DurableEntityRetry.ExecuteAsync(
            nameof(UpdateTransferStatus),
            () => client.Entities.SignalEntityAsync(entityId, nameof(TransferEntityState.UpdateStatus), updateStatusPayload.Status, null, cancellationToken),
            NullLogger.Instance,
            cancellationToken);
    }
}