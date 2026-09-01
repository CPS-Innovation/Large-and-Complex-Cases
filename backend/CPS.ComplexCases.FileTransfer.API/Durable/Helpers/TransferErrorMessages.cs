using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Helpers;

public static class TransferErrorMessages
{
    public static string GetUserMessage(TransferErrorCode errorCode) => errorCode switch
    {
        TransferErrorCode.FileExists =>
            "A file with the same name already exists at the destination.",
        TransferErrorCode.IntegrityVerificationFailed =>
            "The file was uploaded but failed integrity verification, so the transfer was not completed.",
        TransferErrorCode.Transient =>
            "The destination service was temporarily unavailable, so the file was not transferred. Please try again.",
        TransferErrorCode.SourceFileNotFound =>
            "The source file could not be found or is not yet available.",
        _ =>
            "The file could not be transferred due to an unexpected error. Please try again, and contact support if the problem continues."
    };
}
