using CPS.ComplexCases.Common.Models.Domain.Enums;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Payloads;

public class TransferFilePayload
{
    public Guid TransferId { get; set; }
    public TransferType TransferType { get; set; }
    public TransferDirection TransferDirection { get; set; }
    public required string SourcePath { get; set; }
    public required string DestinationPath { get; set; }
}