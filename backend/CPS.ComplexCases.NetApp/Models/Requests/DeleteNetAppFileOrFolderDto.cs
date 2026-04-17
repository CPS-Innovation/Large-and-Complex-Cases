using System.Text.Json.Serialization;

namespace CPS.ComplexCases.NetApp.Models.Requests;

public class DeleteNetAppFileOrFolderDto
{
    [JsonPropertyName("path")]
    public required string Path { get; set; }
}