using CPS.ComplexCases.FileTransfer.API.Durable.Helpers;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

public class FinalizeTransfer(ILogger<FinalizeTransfer> logger)
{
    private readonly ILogger<FinalizeTransfer> _logger = logger;

    [Function(nameof(FinalizeTransfer))]
    public async Task Run([ActivityTrigger] FinalizeTransferPayload finalizeTransferPayload,
        [DurableClient] DurableTaskClient client, CancellationToken cancellationToken = default)
    {
        var entityId = new EntityInstanceId(nameof(TransferEntityState), finalizeTransferPayload.TransferId.ToString());

        await DurableEntityRetry.ExecuteAsync(
            nameof(FinalizeTransfer),
            () => client.Entities.SignalEntityAsync(entityId, nameof(TransferEntityState.FinalizeTransfer),
                cancellationToken),
            _logger,
            cancellationToken);
    }
}