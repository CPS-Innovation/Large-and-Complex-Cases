using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Payloads;

public class UpdateTransferStatusPayload
{
    public Guid TransferId { get; set; }
    public TransferStatus Status { get; set; }
}