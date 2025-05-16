using CPS.ComplexCases.Common.Models.Domain.Enums;

namespace CPS.ComplexCases.API.Domain.Request;

public class InitiateTransferRequest
{
    public TransferType TransferType { get; set; }
    public required List<string> SourcePaths { get; set; }
    public required string DestinationPath { get; set; }
}