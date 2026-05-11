namespace CPS.ComplexCases.FileTransfer.API.Durable.Payloads;

public interface IBatchFileItem
{
    string SourceKey { get; }
    string DestinationPrefix { get; }
    string DestinationFileName { get; }
}

public interface IBatchPayload<TFileItem> where TFileItem : IBatchFileItem
{
    Guid TransferId { get; }
    int CaseId { get; }
    string? UserName { get; }
    Guid? CorrelationId { get; }
    string BearerToken { get; }
    string BucketName { get; }
    List<TFileItem> Files { get; }
    Guid ManageMaterialsOperationId { get; }
}
