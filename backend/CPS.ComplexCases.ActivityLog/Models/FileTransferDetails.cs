using ByteSizeLib;
using CPS.ComplexCases.Common.Models.Domain.Enums;

namespace CPS.ComplexCases.ActivityLog.Models;

public class FileTransferDetails
{
    public required string TransferId { get; set; }
    public required string TransferDirection { get; set; }
    public int TransferedFileCount => Files.Count;
    public int ErrorFileCount => Errors.Count;
    public string TotalSizeTransferred => GetTotalSizeTransferred();
    public long TotalBytesTransferred => GetTotalBytesTransferred();
    public required List<FileTransferItem> Files { get; set; } = [];
    public required List<FileTransferError> Errors { get; set; } = [];

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
}

public class FileTransferError
{
    public required string Path { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}