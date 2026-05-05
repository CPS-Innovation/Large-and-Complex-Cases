using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Data.Models.Requests;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NetAppMoveOperationType
{
    Material,
    Folder
}

public class MoveNetAppBatchDto
{
    [JsonPropertyName("caseId")]
    public required int CaseId { get; set; }

    [JsonPropertyName("destinationPrefix")]
    public required string DestinationPrefix { get; set; }

    [JsonPropertyName("operations")]
    public required List<MoveNetAppBatchOperationDto> Operations { get; set; }
}

public class MoveNetAppBatchOperationDto
{
    [JsonPropertyName("type")]
    public required NetAppMoveOperationType Type { get; set; }

    [JsonPropertyName("sourcePath")]
    public required string SourcePath { get; set; }
}
