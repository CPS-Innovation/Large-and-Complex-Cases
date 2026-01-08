using CPS.ComplexCases.Common.Models.Domain.Enums;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Payloads;

public class DeleteFilesPayload
{
    public Guid TransferId { get; set; }
    public TransferDirection TransferDirection { get; set; }
    public string? WorkspaceId { get; set; } = null;
    public required string UserName { get; set; }
    public Guid? CorrelationId { get; set; } = null;
}