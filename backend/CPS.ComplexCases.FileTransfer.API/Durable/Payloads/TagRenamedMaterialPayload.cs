namespace CPS.ComplexCases.FileTransfer.API.Durable.Payloads;

public class TagRenamedMaterialPayload
{
    public required string BearerToken { get; set; }
    public required string BucketName { get; set; }
    public required string DestinationKey { get; set; }
    public required string OriginalKey { get; set; }
    public string? UserName { get; set; }
    public Guid? CorrelationId { get; set; }
}
