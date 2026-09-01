namespace CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;

public class TransferRetryState
{
    public int RetryAttempt { get; set; }
    public int MaxRetryAttempts { get; set; }
    public int RetryingFileCount { get; set; }
    public int RetryDelaySeconds { get; set; }

    // Non-null while the orchestrator is waiting in the backoff timer; null once the attempt is executing.
    public DateTime? NextRetryAt { get; set; }
}
