using System.Text.Json.Serialization;

namespace CPS.ComplexCases.NetApp.Models.Dto;

public class RegenerateKeysDto
{
    [JsonPropertyName("regenerate_keys")]
    public required string RegenerateKeys { get; set; }
    [JsonPropertyName("key_time_to_live")]
    public required string KeyTimeToLive { get; set; }
}