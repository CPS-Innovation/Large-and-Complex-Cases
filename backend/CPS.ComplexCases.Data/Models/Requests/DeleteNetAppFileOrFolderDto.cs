using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Data.Models.Requests;

public class DeleteNetAppFileOrFolderDto
{
    [JsonPropertyName("path")]
    public required string Path { get; set; }
}