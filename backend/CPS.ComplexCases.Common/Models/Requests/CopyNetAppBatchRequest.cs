using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Common.Models.Requests;

public class CopyNetAppBatchRequest
{
    [JsonPropertyName("caseId")]
    public int CaseId { get; set; }

    [JsonPropertyName("destinationPrefix")]
    public required string DestinationPrefix { get; set; }

    [JsonPropertyName("operations")]
    public required List<CopyNetAppBatchOperationRequest> Operations { get; set; }

    [JsonPropertyName("bearerToken")]
    public required string BearerToken { get; set; }

    [JsonPropertyName("bucketName")]
    public required string BucketName { get; set; }

    [JsonPropertyName("userName")]
    public string? UserName { get; set; }
}

public class CopyNetAppBatchOperationRequest
{
    [JsonPropertyName("type")]
    public required string Type { get; set; }

    [JsonPropertyName("sourcePath")]
    public required string SourcePath { get; set; }
}
