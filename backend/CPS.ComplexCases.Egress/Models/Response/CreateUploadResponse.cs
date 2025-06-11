using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Egress.Models.Response;

public class CreateUploadResponse
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [JsonPropertyName("md5_hash")]
    public string? Md5Hash { get; set; }
}