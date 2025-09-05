using System.Text.Json.Serialization;
using CPS.ComplexCases.Common.Models.Domain.Enums;

namespace CPS.ComplexCases.API.Domain.Request;

public class InitiateTransferRequest
{
    [JsonPropertyName("transferType")]
    public TransferType TransferType { get; set; }
    [JsonPropertyName("transferDirection")]
    public TransferDirection TransferDirection { get; set; }
    [JsonPropertyName("sourcePaths")]
    public required List<SourcePath> SourcePaths { get; set; }
    [JsonPropertyName("sourceRootFolderPath")]
    public string? SourceRootFolderPath { get; set; }
    [JsonPropertyName("destinationPath")]
    public required string DestinationPath { get; set; }
    [JsonPropertyName("caseId")]
    public required int CaseId { get; set; }
    [JsonPropertyName("workspaceId")]
    public required string WorkspaceId { get; set; }
    [JsonPropertyName("isRetry")]
    public bool? IsRetry { get; set; } = false;
}

public class SourcePath
{
    [JsonPropertyName("path")]
    public required string Path { get; set; }
    [JsonPropertyName("fileId")]
    public string? FileId { get; set; }
    [JsonPropertyName("isFolder")]
    public bool? IsFolder { get; set; }
    [JsonPropertyName("relativePath")]
    public string? RelativePath { get; set; }
    [JsonPropertyName("fullFilePath")]
    public string? FullFilePath { get; set; }
}