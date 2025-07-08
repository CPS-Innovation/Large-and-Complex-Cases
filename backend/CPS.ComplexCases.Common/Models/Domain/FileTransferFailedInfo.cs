using CPS.ComplexCases.Common.Models.Domain.Enums;

namespace CPS.ComplexCases.Common.Models.Domain;

public class FileTransferFailedInfo : FileTransferInfo
{
    public string Message { get; set; } = string.Empty;
    public TransferFailedType ErrorType { get; set; }
}