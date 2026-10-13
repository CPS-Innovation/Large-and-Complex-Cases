using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Entities;

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

    public void UpdateStatus(UpdateTransferStatusPayload payload)
    {
        State.Status = payload.Status;
        if (!string.IsNullOrWhiteSpace(payload.ErrorMessage))
        {
            State.ErrorMessage = payload.ErrorMessage;
        }

        State.UpdatedAt = DateTime.UtcNow;
    }

    public void SetErrorMessage(string errorMessage)
    {
        State.ErrorMessage = errorMessage;
        State.UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateRetryState(TransferRetryState retryState)
    {
        State.RetryState = retryState;
        State.UpdatedAt = DateTime.UtcNow;
    }

    public void ClearRetryState()
    {
        State.RetryState = null;
        State.UpdatedAt = DateTime.UtcNow;
    }

    public void FinalizeTransfer()
    {
        // partially completed if any copy failed OR any source delete failed
        State.Status = (State.FailedItems.Count > 0 || State.DeletionErrors.Count > 0)
            ? TransferStatus.PartiallyCompleted
            : TransferStatus.Completed;
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

    public void AddSkippedItem(TransferItem skippedItem)
    {
        State.SkippedItems.Add(skippedItem);
        State.SkippedFiles++;
        State.ProcessedFiles++;
        State.UpdatedAt = DateTime.UtcNow;
    }

    public void DeleteMovedItemsCompleted(List<DeletionError> failedToDeleteItems)
    {
        State.MovedFilesDeletedSuccessfully = failedToDeleteItems.Count == 0;
        State.DeletionErrors.AddRange(failedToDeleteItems);
        State.UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveTransientFailures()
    {
        var transientFailures = State.FailedItems
            .Where(f => f.ErrorCode == TransferErrorCode.Transient)
            .ToList();

        foreach (var failure in transientFailures)
        {
            State.FailedItems.Remove(failure);
            State.FailedFiles--;
            // ProcessedFiles deliberately NOT decremented
            // the file was already "processed" from the UI's perspective
        }

        State.UpdatedAt = DateTime.UtcNow;
    }

    // Egress can accept and acknowledge an upload without the file ever materialising in the
    // destination folder. When the post-transfer verification pass finds such a file, move it out of
    // the successful items so the UI does not report it as transferred and the orchestrator's
    // transient retry pass can re-queue it.
    public void DemoteSuccessfulItems(DemoteSuccessfulItemsPayload payload)
    {
        foreach (var sourcePath in payload.SourcePaths ?? [])
        {
            var item = State.SuccessfulItems
                .FirstOrDefault(i => string.Equals(i.SourcePath, sourcePath, StringComparison.OrdinalIgnoreCase));

            if (item is null)
            {
                continue;
            }

            State.SuccessfulItems.Remove(item);
            State.SuccessfulFiles--;

            State.FailedItems.Add(new TransferFailedItem
            {
                SourcePath = sourcePath,
                Status = TransferItemStatus.Failed,
                ErrorCode = TransferErrorCode.Transient,
                ErrorMessage = payload.ErrorMessage
            });
            State.FailedFiles++;
            // ProcessedFiles deliberately NOT incremented
            // the file was already counted as processed on the first pass
        }

        State.UpdatedAt = DateTime.UtcNow;
    }

    public void AddSuccessfulRetryItem(TransferItem transferItem)
    {
        State.SuccessfulItems.Add(transferItem);
        State.SuccessfulFiles++;
        // ProcessedFiles NOT incremented -- already counted from first attempt
        State.UpdatedAt = DateTime.UtcNow;
    }

    public void AddFailedRetryItem(TransferFailedItem failedItem)
    {
        State.FailedItems.Add(failedItem);
        State.FailedFiles++;
        // ProcessedFiles NOT incremented -- already counted from first attempt
        State.UpdatedAt = DateTime.UtcNow;
    }

    public void ApplyResultBatch(TransferResultBatch batch)
    {
        foreach (var item in batch.SuccessfulItems ?? [])
        {
            if (batch.IsRetry)
            {
                AddSuccessfulRetryItem(item);
            }
            else
            {
                AddSuccessfulItem(item);
            }
        }

        // Skipped items have no retry variant. A skip still counts as processed.
        foreach (var item in batch.SkippedItems ?? [])
        {
            AddSkippedItem(item);
        }

        foreach (var item in batch.FailedItems ?? [])
        {
            if (batch.IsRetry)
            {
                AddFailedRetryItem(item);
            }
            else
            {
                AddFailedItem(item);
            }
        }
    }

    public TransferEntity CurrentState => State;

}
