using CPS.ComplexCases.Common.Models.Domain.Enums;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Payloads;

public class SendTransferNotificationPayload
{
    public Guid TransferId { get; set; }
    public int CaseId { get; set; }
    public string? Urn { get; set; }
    public string? UserName { get; set; }
    public Guid? CorrelationId { get; set; }
    public bool IsFailure { get; set; }
    public TransferDirection TransferDirection { get; set; }
    public TransferType TransferType { get; set; }
}
