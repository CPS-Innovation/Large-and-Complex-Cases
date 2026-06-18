using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Client.Entities;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Helpers;

public class TransferEntityHelper(
    ILogger<TransferEntityHelper> logger) : ITransferEntityHelper
{
    private readonly ILogger<TransferEntityHelper> _logger = logger;

    public Task DeleteMovedItemsCompleted(DurableTaskClient client, Guid transferId, List<DeletionError> failedItems,
        CancellationToken cancellationToken = default)
    {
        var entityId = GetEntityInstanceId(transferId);

        _logger.LogInformation(
            "Signalling Durable entity {EntityName}/{EntityKey} for DeleteMovedItemsCompleted. TransferId={TransferId}, FailedItemCount={FailedCount}",
            entityId.Name, entityId.Key, transferId, failedItems.Count);

        return DurableEntityRetry.ExecuteAsync(
            nameof(DeleteMovedItemsCompleted),
            () => client.Entities.SignalEntityAsync(entityId, nameof(TransferEntityState.DeleteMovedItemsCompleted),
                failedItems, null, cancellationToken),
            _logger,
            cancellationToken);
    }

    public async Task<EntityMetadata<TransferEntity>?> GetTransferEntityAsync(DurableTaskClient client, Guid transferId,
        CancellationToken cancellationToken = default)
    {
        var entityId = GetEntityInstanceId(transferId);

        _logger.LogInformation(
            "Getting Durable entity {EntityName}/{EntityKey} for TransferId={TransferId}",
            entityId.Name, entityId.Key, transferId);

        var entity = await DurableEntityRetry.ExecuteAsync(
            nameof(GetTransferEntityAsync),
            () => client.Entities.GetEntityAsync<TransferEntity>(entityId, cancellationToken),
            _logger,
            cancellationToken);

        _logger.LogInformation(
            "Got Durable entity {EntityName}/{EntityKey} for TransferId={TransferId}. EntityFound={EntityFound}, HasState={HasState}",
            entityId.Name,
            entityId.Key,
            transferId,
            entity is not null,
            entity?.State is not null);

        return entity;
    }

    private static EntityInstanceId GetEntityInstanceId(Guid transferId)
    {
        return new EntityInstanceId(nameof(TransferEntityState), transferId.ToString());
    }
}