using System.Text.Json.Serialization;

namespace CPS.ComplexCases.NetApp.Models.Requests;

public class GetFileLockRequestDto
{
    [JsonPropertyName("path")]
    public required string Path { get; set; }
}