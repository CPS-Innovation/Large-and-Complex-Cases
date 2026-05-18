using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Data.Models.Requests;

public class CopyNetAppBatchDto : INetAppBatchDto<CopyNetAppBatchOperationDto>
{
    [JsonPropertyName("caseId")]
    public required int CaseId { get; set; }

    [JsonPropertyName("destinationPrefix")]
    public required string DestinationPrefix { get; set; }

    [JsonPropertyName("operations")]
    public required List<CopyNetAppBatchOperationDto> Operations { get; set; }
}

public class CopyNetAppBatchOperationDto : INetAppBatchOperationDto
{
    [JsonPropertyName("type")]
    public required NetAppBatchOperationType Type { get; set; }

    [JsonPropertyName("sourcePath")]
    public required string SourcePath { get; set; }
}
