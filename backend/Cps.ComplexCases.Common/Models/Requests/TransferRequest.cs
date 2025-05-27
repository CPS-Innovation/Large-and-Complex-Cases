using CPS.ComplexCases.Common.Models.Domain.Enums;

namespace CPS.ComplexCases.Common.Models.Requests;

public class TransferRequest
{
    public TransferType TransferType { get; set; }
    public TransferDirection TransferDirection { get; set; }
    public required List<TransferSourcePath> SourcePaths { get; set; }
    public required string DestinationPath { get; set; }
    public required TransferMetadata Metadata { get; set; }
}

public class TransferMetadata
{
    public int CaseId { get; set; }
    public required string UserName { get; set; }
    public required string WorkspaceId { get; set; }
}

public class TransferSourcePath
{
    public required string Path { get; set; }
    public string? FileId { get; set; }
}