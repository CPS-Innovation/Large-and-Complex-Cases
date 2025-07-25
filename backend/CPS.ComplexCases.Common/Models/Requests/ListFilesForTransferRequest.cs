using System.Text.Json.Serialization;
using CPS.ComplexCases.Common.Models.Domain.Enums;

namespace CPS.ComplexCases.Common.Models.Requests;

public class ListFilesForTransferRequest
{
    [JsonPropertyName("caseId")]
    public required int CaseId { get; set; }
    [JsonPropertyName("transferDirection")]
    public required TransferDirection TransferDirection { get; set; }
    [JsonPropertyName("transferType")]
    public required TransferType TransferType { get; set; }
    [JsonPropertyName("sourcePaths")]
    public required List<SelectedSourcePath> SourcePaths { get; set; }
    [JsonPropertyName("destinationPath")]
    public required string DestinationPath { get; set; }
    [JsonPropertyName("workspaceId")]
    public string? WorkspaceId { get; set; }
    [JsonPropertyName("correlationId")]
    public Guid? CorrelationId { get; set; }
    [JsonPropertyName("username")]
    public string? Username { get; set; }
}

public class SelectedSourcePath
{
    [JsonPropertyName("path")]
    public required string Path { get; set; }
    [JsonPropertyName("fileId")]
    public string? FileId { get; set; }
    [JsonPropertyName("isFolder")]
    public bool? IsFolder { get; set; }
}