namespace CPS.ComplexCases.FileTransfer.API.Durable.Payloads;

public class WriteMoveActivityLogPayload
{
    public int CaseId { get; set; }
    public string? UserName { get; set; }
    public Guid? CorrelationId { get; set; }
    public required List<MoveBatchOriginalOperation> OriginalOperations { get; set; }
    public required List<string> SuccessfulSourceKeys { get; set; }
}
