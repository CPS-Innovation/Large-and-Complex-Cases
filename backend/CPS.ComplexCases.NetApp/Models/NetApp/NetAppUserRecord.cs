using System.Text.Json.Serialization;

namespace CPS.ComplexCases.NetApp.Models.NetApp;

public class NetAppUserRecord
{
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    [JsonPropertyName("access_key")]
    public string? AccessKey { get; set; }
    [JsonPropertyName("secret_key")]
    public string? SecretKey { get; set; }
}