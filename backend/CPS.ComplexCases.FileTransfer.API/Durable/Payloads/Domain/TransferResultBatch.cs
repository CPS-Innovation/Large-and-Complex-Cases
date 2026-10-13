namespace CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;

/// <summary>
/// One entity update for a completed transfer batch. Successful, skipped, and failed
/// items are applied together so the orchestrator checkpoints once per batch.
/// </summary>
public class TransferResultBatch
{
    public bool IsRetry { get; set; }
    public List<TransferItem> SuccessfulItems { get; set; } = [];
    public List<TransferItem> SkippedItems { get; set; } = [];
    public List<TransferFailedItem> FailedItems { get; set; } = [];

    public bool HasItems =>
        SuccessfulItems.Count > 0 || SkippedItems.Count > 0 || FailedItems.Count > 0;
}
