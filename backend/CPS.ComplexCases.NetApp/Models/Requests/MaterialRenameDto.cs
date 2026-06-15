using System.Text.Json.Serialization;

namespace CPS.ComplexCases.NetApp.Models.Requests;

public class MaterialRenameDto
{
    [JsonPropertyName("caseId")]
    public required int CaseId { get; set; }
    [JsonPropertyName("currentPath")]
    public required string CurrentPath { get; set; }
    [JsonPropertyName("newPath")]
    public required string NewPath { get; set; }
}