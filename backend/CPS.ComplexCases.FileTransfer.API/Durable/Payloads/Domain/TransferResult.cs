namespace CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;

/// <summary>
/// Result returned by TransferFile activity
/// </summary>
public class TransferResult
{
    public bool IsSuccess { get; set; }
    public bool IsSkipped { get; set; }
    public TransferItem? SuccessfulItem { get; set; }
    public TransferItem? SkippedItem { get; set; }
    public TransferFailedItem? FailedItem { get; set; }
}