using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Entities;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;

namespace CPS.ComplexCases.FileTransfer.API.Durable.State;

public class TransferEntityState : TaskEntity<TransferEntity>
{
    [Function(nameof(TransferEntityState))]
    public void RunEntity([EntityTrigger] TaskEntityDispatcher entityDispatcher)
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

    public void FinalizeTransfer()
    {
        // if any items failed then its partially completed, if all items succeeded then its completed
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

    public void DeleteMovedItemsCompleted(List<DeletionError> failedToDeleteItems)
    {
        State.MovedFilesDeletedSuccessfully = failedToDeleteItems.Count == 0;
        State.DeletionErrors.AddRange(failedToDeleteItems);
        State.UpdatedAt = DateTime.UtcNow;
    }
}