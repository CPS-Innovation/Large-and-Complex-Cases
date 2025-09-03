using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Models.Requests;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Payloads;

public class TransferPayload
{
    public Guid TransferId { get; set; }
    public int CaseId { get; set; }
    public string? UserName { get; set; }
    public TransferType TransferType { get; set; }
    public TransferDirection TransferDirection { get; set; }
    public required List<TransferSourcePath> SourcePaths { get; set; }
    public required string DestinationPath { get; set; }
    public required string WorkspaceId { get; set; }
    public bool? IsRetry { get; set; } = false;
    public Guid? CorrelationId { get; set; } = null;
    public string? SourceRootFolder { get; set; } = null;
}