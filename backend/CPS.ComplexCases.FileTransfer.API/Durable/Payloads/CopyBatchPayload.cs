namespace CPS.ComplexCases.FileTransfer.API.Durable.Payloads;

public class CopyBatchPayload
{
    public Guid TransferId { get; set; }
    public int CaseId { get; set; }
    public string? UserName { get; set; }
    public Guid? CorrelationId { get; set; }
    public required string BearerToken { get; set; }
    public required string BucketName { get; set; }
    public required List<CopyFileItem> Files { get; set; }
    public required List<CopyBatchOriginalOperation> OriginalOperations { get; set; }
    public Guid ManageMaterialsOperationId { get; set; }
}

public class CopyFileItem
{
    public required string SourceKey { get; set; }
    public required string DestinationPrefix { get; set; }
    public required string DestinationFileName { get; set; }
}

public class CopyBatchOriginalOperation
{
    public required string Type { get; set; }
    public required string SourcePath { get; set; }
}
