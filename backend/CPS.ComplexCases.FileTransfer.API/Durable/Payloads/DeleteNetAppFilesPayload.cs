namespace CPS.ComplexCases.FileTransfer.API.Durable.Payloads;

public class DeleteNetAppFilesPayload
{
    public Guid TransferId { get; set; }
    public required string BearerToken { get; set; }
    public required string BucketName { get; set; }
    public required string UserName { get; set; }
    public Guid? CorrelationId { get; set; } = null;
    public int? CaseId { get; set; } = null;
}
