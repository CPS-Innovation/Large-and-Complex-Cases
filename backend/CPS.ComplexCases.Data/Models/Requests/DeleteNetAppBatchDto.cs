using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Data.Models.Requests;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NetAppDeleteOperationType
{
    Material,
    Folder
}

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
    public required NetAppDeleteOperationType Type { get; set; }

    [JsonPropertyName("sourcePath")]
    public required string SourcePath { get; set; }
}
