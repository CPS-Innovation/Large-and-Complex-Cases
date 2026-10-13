namespace CPS.ComplexCases.Common.Models.Domain.Exceptions;

/// <summary>
/// Thrown when a storage client exhausts its own in-client retries against a destination that kept
/// returning retryable errors. It marks the failure as worth another attempt later, so the transfer
/// orchestrator re-queues the file instead of reporting it as a permanent failure.
/// </summary>
[Serializable]
public class TransientStorageException : Exception
{
    public TransientStorageException(string message)
        : base(message)
    {
    }

    public TransientStorageException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
