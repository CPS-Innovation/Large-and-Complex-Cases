using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Data.Models.Requests;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NetAppCopyOperationType
{
    Material,
    Folder
}

public class CopyNetAppBatchDto
{
    [JsonPropertyName("caseId")]
    public required int CaseId { get; set; }

    [JsonPropertyName("destinationPrefix")]
    public required string DestinationPrefix { get; set; }

    [JsonPropertyName("operations")]
    public required List<CopyNetAppBatchOperationDto> Operations { get; set; }
}

public class CopyNetAppBatchOperationDto
{
    [JsonPropertyName("type")]
    public required NetAppCopyOperationType Type { get; set; }

    [JsonPropertyName("sourcePath")]
    public required string SourcePath { get; set; }
}
