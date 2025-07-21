using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Client.Entities;
using Microsoft.DurableTask.Entities;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Helpers;

public class TransferEntityHelper(DurableTaskClient durableClient) : ITransferEntityHelper
{
    private readonly DurableTaskClient _durableClient = durableClient;

    public Task DeleteMovedItemsCompleted(Guid transferId, List<DeletionError> failedItems, CancellationToken cancellationToken = default)
    {
        var entityId = GetEntityInstanceId(transferId);
        return _durableClient.Entities.SignalEntityAsync(entityId, nameof(TransferEntityState.DeleteMovedItemsCompleted), failedItems, null, cancellationToken);
    }

    public async Task<EntityMetadata<TransferEntity>?> GetTransferEntityAsync(Guid transferId, CancellationToken cancellationToken = default)
    {
        var entityId = GetEntityInstanceId(transferId);
        var entity = await _durableClient.Entities.GetEntityAsync<TransferEntity>(entityId, cancellationToken);
        return entity ?? null;
    }

    private static EntityInstanceId GetEntityInstanceId(Guid transferId)
    {
        return new EntityInstanceId(nameof(TransferEntityState), transferId.ToString());
    }
}