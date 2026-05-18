using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Data.Models.Requests;

public class MoveNetAppBatchDto : INetAppBatchDto<MoveNetAppBatchOperationDto>
{
    [JsonPropertyName("caseId")]
    public required int CaseId { get; set; }

    [JsonPropertyName("destinationPrefix")]
    public required string DestinationPrefix { get; set; }

    [JsonPropertyName("operations")]
    public required List<MoveNetAppBatchOperationDto> Operations { get; set; }
}

public class MoveNetAppBatchOperationDto : INetAppBatchOperationDto
{
    [JsonPropertyName("type")]
    public required NetAppBatchOperationType Type { get; set; }

    [JsonPropertyName("sourcePath")]
    public required string SourcePath { get; set; }
}
