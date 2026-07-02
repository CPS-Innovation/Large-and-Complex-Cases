using System.Text.Json.Serialization;
using CPS.ComplexCases.Data.Enums;

namespace CPS.ComplexCases.Data.Models.Requests;

public class DeleteNetAppBatchDto
{
    [JsonPropertyName("caseId")]
    public required int CaseId { get; set; }

    [JsonPropertyName("operations")]
    public required List<DeleteNetAppBatchOperationDto> Operations { get; set; }
}

public class DeleteNetAppBatchOperationDto
{
    [JsonPropertyName("type")]
    public required NetAppOperationType Type { get; set; }

    [JsonPropertyName("sourcePath")]
    public required string SourcePath { get; set; }
}
