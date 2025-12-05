using ByteSizeLib;
using DomainEnums = CPS.ComplexCases.Common.Models.Domain.Enums;

namespace CPS.ComplexCases.ActivityLog.Models;

public class FileTransferDetails
{
    public required string TransferId { get; set; }
    public required string TransferDirection { get; set; }
    public required string TransferType { get; set; }
    public required string SourcePath { get; set; }
    public required string DestinationPath { get; set; }
    public int TotalFiles { get; set; }
    public int TransferedFileCount => Files.Count;
    public int ErrorFileCount => Errors.Count;
    public bool IsCompleted => TransferedFileCount == TotalFiles && ErrorFileCount == 0;
    public bool SourceFilesDeletedSuccessfully => TransferType == nameof(DomainEnums.TransferType.Move) &&
                                                  TransferDirection == nameof(DomainEnums.TransferDirection.EgressToNetApp) &&
                                                  DeletionErrors.Count == 0;
    public string TotalSizeTransferred => GetTotalSizeTransferred();
    public long TotalBytesTransferred => GetTotalBytesTransferred();
    public required List<FileTransferItem> Files { get; set; } = [];
    public required List<FileTransferError> Errors { get; set; } = [];
    public List<FileTransferError> DeletionErrors { get; set; } = [];
    public string? ExceptionMessage { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    private string GetTotalSizeTransferred()
    {
        if (Files.Count == 0)
            return "0 B";

        long totalSize = Files.Sum(file => file.Size);
        return ByteSize.FromBytes(totalSize).ToString();
    }

    private long GetTotalBytesTransferred()
    {
        if (Files.Count == 0)
            return 0;

        return Files.Sum(file => file.Size);
    }
}

public class FileTransferItem
{
    public required string Path { get; set; }
    public required bool IsRenamed { get; set; }
    public required long Size { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? MD5Hash { get; set; }
}

public class FileTransferError
{
    public required string Path { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}