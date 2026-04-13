using System.Text.Json.Serialization;

namespace CPS.ComplexCases.API.Domain.Request;

public class RenameNetAppMaterialRequest
{
    [JsonPropertyName("caseId")]
    public required int CaseId { get; set; }

    [JsonPropertyName("sourcePath")]
    public required string SourcePath { get; set; }

    [JsonPropertyName("destinationPath")]
    public required string DestinationPath { get; set; }
}
