using ByteSizeLib;
using CPS.ComplexCases.Common.Models.Domain.Enums;

namespace CPS.ComplexCases.ActivityLog.Models;

public class FileTransferDetails
{
    public FileTransferDetails(string transferId, TransferDirection transferDirection)
    {
        TransferId = transferId;
        TransferDirection = transferDirection;
    }

    public required string TransferId { get; set; }
    public required TransferDirection TransferDirection { get; set; }
    public int TransferedFileCount => Files.Count;
    public int ErrorFileCount => Errors.Count;
    public string TotalSizeTransferred => GetTotalSizeTransferred();
    public required List<FileTransferItem> Files { get; set; } = [];
    public required List<FileTransferItem> Errors { get; set; } = [];

    private string GetTotalSizeTransferred()
    {
        if (Files == null || Files.Count == 0)
            return "0 B";

        long totalSize = Files.Sum(file => file.Size);
        return ByteSize.FromMegaBytes(totalSize).ToString();
    }
}

public class FileTransferItem
{
    public required string SourcePath { get; set; }
    public required string Status { get; set; }
    public required bool IsRenamed { get; set; }
    public required long Size { get; set; }
}