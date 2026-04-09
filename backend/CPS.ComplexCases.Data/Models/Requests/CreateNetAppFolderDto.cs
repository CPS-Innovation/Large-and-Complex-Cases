using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Data.Models.Requests;

public class CreateNetAppFolderDto
{
    [JsonPropertyName("path")]
    public required string Path { get; set; }

    [JsonPropertyName("caseId")]
    public required int CaseId { get; set; }
}
