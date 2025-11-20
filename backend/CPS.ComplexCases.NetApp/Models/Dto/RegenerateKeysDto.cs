using System.Text.Json.Serialization;

namespace CPS.ComplexCases.NetApp.Models.Dto;

public class RegenerateKeysDto
{
    [JsonPropertyName("regenerate_keys")]
    public required string RegenerateKeys { get; set; }
}