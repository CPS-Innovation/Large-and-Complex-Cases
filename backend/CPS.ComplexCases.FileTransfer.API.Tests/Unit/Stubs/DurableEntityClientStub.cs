using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client.Entities;
using Microsoft.DurableTask.Entities;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Stubs;

public class DurableEntityClientStub : DurableEntityClient
{
    public DurableEntityClientStub(string name) : base(name)
    {
    }

    public Func<EntityInstanceId, CancellationToken, Task<EntityMetadata<TransferEntity>>>? OnGetEntityAsync { get; set; }

    public override Task<CleanEntityStorageResult> CleanEntityStorageAsync(CleanEntityStorageRequest? request = null, bool continueUntilComplete = true, CancellationToken cancellation = default)
    {
        throw new NotImplementedException();
    }

    public override AsyncPageable<EntityMetadata> GetAllEntitiesAsync(EntityQuery? filter = null)
    {
        throw new NotImplementedException();
    }

    public override AsyncPageable<EntityMetadata<T>> GetAllEntitiesAsync<T>(EntityQuery? filter = null)
    {
        throw new NotImplementedException();
    }

    public override Task<EntityMetadata<T>?> GetEntityAsync<T>(EntityInstanceId id, CancellationToken cancellationToken = default)
    {
        if (typeof(T) == typeof(TransferEntity))
        {
            if (OnGetEntityAsync == null)
            {
                throw new InvalidOperationException("OnGetEntityAsync delegate is not set.");
            }
            return (Task<EntityMetadata<T>?>)(object)OnGetEntityAsync(id, cancellationToken);
        }

        throw new NotSupportedException("Only TransferEntity is supported in the stub.");
    }

    public override Task<EntityMetadata?> GetEntityAsync(EntityInstanceId id, bool includeState = true, CancellationToken cancellation = default)
    {
        throw new NotImplementedException();
    }

    public override Task<EntityMetadata<T>?> GetEntityAsync<T>(
        EntityInstanceId id, bool includeState = true, CancellationToken cancellation = default)
    {
        if (typeof(T) == typeof(TransferEntity))
        {
            if (OnGetEntityAsync == null)
            {
                throw new InvalidOperationException("OnGetEntityAsync delegate is not set.");
            }
            return (Task<EntityMetadata<T>?>)(object)OnGetEntityAsync(id, cancellation);
        }

        throw new NotSupportedException("Only TransferEntity is supported in the stub.");
    }

    public override Task SignalEntityAsync(EntityInstanceId id, string operationName, object? input = null, SignalEntityOptions? options = null, CancellationToken cancellation = default)
    {
        throw new NotImplementedException();
    }

}