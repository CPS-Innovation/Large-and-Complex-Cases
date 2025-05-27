using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Entities;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;

namespace CPS.ComplexCases.FileTransfer.API.Durable.State;

public class TransferEntityState : TaskEntity<TransferEntity>
{
    [Function(nameof(TransferEntityState))]
    public void RunEntityAsync([EntityTrigger] TaskEntityDispatcher entityDispatcher)
    {
        entityDispatcher.DispatchAsync(this);
    }

    public void Initialize(TransferEntity entity)
    {
        State = entity;
    }

    public void UpdateStatus(TransferStatus status)
    {
        State.Status = status;
        State.UpdatedAt = DateTime.UtcNow;
    }
}