namespace CPS.ComplexCases.FileTransfer.API.Durable.Payloads;

public class DeleteNetAppSourceFoldersPayload
{
    public Guid TransferId { get; set; }
    public required string BearerToken { get; set; }
    public required string BucketName { get; set; }
    public required string UserName { get; set; }
    public Guid? CorrelationId { get; set; } = null;
    public int? CaseId { get; set; } = null;
    public required List<string> SourceFolderPaths { get; set; }
}
