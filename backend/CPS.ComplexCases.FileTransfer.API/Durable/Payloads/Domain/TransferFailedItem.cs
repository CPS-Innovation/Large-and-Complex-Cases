
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;

public class TransferFailedItem
{
    public required string SourcePath { get; set; }
    public TransferStatus Status { get; set; }
    public TransferErrorCode ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}
