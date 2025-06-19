using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;

public class TransferItem
{
    public required string SourcePath { get; set; }
    public required TransferItemStatus Status { get; set; }
    public required bool IsRenamed { get; set; }
    public required long Size { get; set; }
}