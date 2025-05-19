using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;

namespace CPS.ComplexCases.FileTransfer.API.Models.Domain;

public class Transfer
{
    public required string TransferId { get; set; }
    public int CaseId { get; set; }
    public TransferStatus Status { get; set; }
    public TransferType TransferType { get; set; }
    public required string DestinationPath { get; set; }
    public DateTime CreatedAt { get; set; }
}