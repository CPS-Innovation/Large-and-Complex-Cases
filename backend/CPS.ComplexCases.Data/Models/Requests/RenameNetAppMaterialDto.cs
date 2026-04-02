using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Data.Models.Requests;

public class RenameNetAppMaterialDto
{
    [JsonPropertyName("caseId")]
    public required int CaseId { get; set; }

    [JsonPropertyName("sourcePath")]
    public required string SourcePath { get; set; }

    [JsonPropertyName("newName")]
    public required string NewName { get; set; }
}
