using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Data.Models.Requests;

public class CreateNetAppConnectionDto
{
    [JsonPropertyName("caseId")]
    public required int CaseId { get; set; }
    [JsonPropertyName("bucketName")]
    public required string BucketName { get; set; }
    [JsonPropertyName("netAppFolderPath")]
    public required string NetAppFolderPath { get; set; }
}
