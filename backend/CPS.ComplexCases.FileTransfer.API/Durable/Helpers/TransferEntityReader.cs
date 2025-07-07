using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Client.Entities;
using Microsoft.DurableTask.Entities;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Helpers;

public class TransferEntityReader(DurableTaskClient durableClient) : ITransferEntityReader
{
    private readonly DurableTaskClient _durableClient = durableClient;

    public async Task<EntityMetadata<TransferEntity>?> GetTransferEntityAsync(Guid transferId, CancellationToken cancellationToken = default)
    {
        var entityId = new EntityInstanceId(nameof(TransferEntityState), transferId.ToString());
        var entity = await _durableClient.Entities.GetEntityAsync<TransferEntity>(entityId, cancellationToken);
        return entity ?? null;
    }
}