using System.Text.Json.Serialization;

namespace CPS.ComplexCases.NetApp.Models.Dto;

public class MaterialRenameDto
{
    [JsonPropertyName("path")]
    public required string Path { get; set; }
}