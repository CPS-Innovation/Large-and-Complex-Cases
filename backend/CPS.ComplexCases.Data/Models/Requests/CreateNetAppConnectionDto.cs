using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Data.Models.Requests;

public class CreateNetAppConnectionDto
{
    [JsonPropertyName("caseId")]
    public required int CaseId { get; set; }
    [JsonPropertyName("operationName")]
    public required string OperationName { get; set; }
    [JsonPropertyName("folderPath")]
    public required string NetAppFolderPath { get; set; }
}
