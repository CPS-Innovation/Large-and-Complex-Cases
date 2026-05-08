namespace CPS.ComplexCases.FileTransfer.API.Durable.Payloads;

public class WriteCopyActivityLogPayload
{
    public int CaseId { get; set; }
    public string? UserName { get; set; }
    public Guid? CorrelationId { get; set; }
    public required List<CopyBatchOriginalOperation> OriginalOperations { get; set; }
    public required List<string> SuccessfulSourceKeys { get; set; }
}
