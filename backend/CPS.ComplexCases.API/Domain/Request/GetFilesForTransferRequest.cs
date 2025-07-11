using System.Text.Json.Serialization;
using CPS.ComplexCases.Common.Models.Domain.Enums;

namespace CPS.ComplexCases.API.Domain.Request;

public class GetFilesForTransferRequest
{
    [JsonPropertyName("caseId")]
    public required int CaseId { get; set; }
    [JsonPropertyName("transferDirection")]
    public TransferDirection TransferDirection { get; set; }
    [JsonPropertyName("transferType")]
    public TransferType TransferType { get; set; }
    [JsonPropertyName("sourcePaths")]
    public required List<SourcePath> SourcePaths { get; set; }
    [JsonPropertyName("destinationPath")]
    public required string DestinationPath { get; set; }
    [JsonPropertyName("workspaceId")]
    public string? WorkspaceId { get; set; }
}