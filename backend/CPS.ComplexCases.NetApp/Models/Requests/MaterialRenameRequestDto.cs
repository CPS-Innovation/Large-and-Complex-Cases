using System.Text.Json.Serialization;
using CPS.ComplexCases.Data.Enums;

namespace CPS.ComplexCases.NetApp.Models.Requests;

public class MaterialRenameRequestDto
{
    [JsonPropertyName("caseId")]
    public required int CaseId { get; set; }
    [JsonPropertyName("operations")]
    public required List<RenameNetAppMaterialBatchOperationDto> Operations { get; set; }
}

public class RenameNetAppMaterialBatchOperationDto
{
    [JsonPropertyName("type")]
    public required NetAppOperationType Type { get; set; }
    [JsonPropertyName("currentPath")]
    public required string CurrentPath { get; set; }
    [JsonPropertyName("newPath")]
    public required string NewPath { get; set; }
}