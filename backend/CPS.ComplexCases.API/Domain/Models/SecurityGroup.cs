using System.Text.Json.Serialization;

namespace CPS.ComplexCases.API.Domain.Models;

public class SecurityGroup
{
    [JsonPropertyName("id")]
    public required Guid Id { get; set; }
    [JsonPropertyName("displayName")]
    public required string DisplayName { get; set; }
    [JsonPropertyName("bucketName")]
    public required string BucketName { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}