using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Helpers;

public interface ITransferEntityHelper
{
    Task<Microsoft.DurableTask.Client.Entities.EntityMetadata<TransferEntity>?> GetTransferEntityAsync(Guid transferId, CancellationToken cancellationToken = default);
    Task DeleteMovedItemsCompleted(Guid transferId, List<FailedToDeleteItem> failedItems, CancellationToken cancellationToken = default);
}