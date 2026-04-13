using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Common.Models.Requests;

public class RenameNetAppMaterialRequest
{
    [JsonPropertyName("caseId")]
    public int CaseId { get; set; }

    [JsonPropertyName("sourcePath")]
    public required string SourcePath { get; set; }

    [JsonPropertyName("destinationPath")]
    public required string DestinationPath { get; set; }

    [JsonPropertyName("bearerToken")]
    public required string BearerToken { get; set; }

    [JsonPropertyName("bucketName")]
    public required string BucketName { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }
}
