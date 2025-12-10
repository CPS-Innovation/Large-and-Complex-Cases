using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Models.Requests;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Payloads;

public class TransferFilePayload
{
    public Guid TransferId { get; set; }
    public TransferType TransferType { get; set; }
    public TransferDirection TransferDirection { get; set; }
    public required TransferSourcePath SourcePath { get; set; }
    public required string DestinationPath { get; set; }
    public required string WorkspaceId { get; set; }
    public string? SourceRootFolderPath { get; set; } = null;
    public required string BearerToken { get; set; }
}