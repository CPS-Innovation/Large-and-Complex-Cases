using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Client.Entities;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Helpers;

public interface ITransferEntityHelper
{
    Task<EntityMetadata<TransferEntity>?> GetTransferEntityAsync(DurableTaskClient client, Guid transferId, CancellationToken cancellationToken = default);
    Task DeleteMovedItemsCompleted(DurableTaskClient client, Guid transferId, List<DeletionError> failedItems, CancellationToken cancellationToken = default);
}