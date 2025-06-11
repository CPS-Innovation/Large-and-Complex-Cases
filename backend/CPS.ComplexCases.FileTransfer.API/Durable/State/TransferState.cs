using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Entities;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;

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

    public void Finalize(FinalizeTransferPayload payload)
    {
        State.Status = State.FailedItems.Count > 0 ? TransferStatus.PartiallyCompleted : TransferStatus.Completed;
        State.CompletedAt = DateTime.UtcNow;
        State.UpdatedAt = DateTime.UtcNow;
    }

    public void AddSuccessfulItem(TransferItem transferItem)
    {
        State.SuccessfulItems.Add(transferItem);
        State.SuccessfulFiles++;
        State.ProcessedFiles++;
        State.UpdatedAt = DateTime.UtcNow;
    }

    public void AddFailedItem(TransferFailedItem failedItem)
    {
        State.FailedItems.Add(failedItem);
        State.FailedFiles++;
        State.ProcessedFiles++;
        State.UpdatedAt = DateTime.UtcNow;
    }
}