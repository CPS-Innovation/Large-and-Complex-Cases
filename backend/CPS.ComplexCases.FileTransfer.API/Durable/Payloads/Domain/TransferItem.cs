using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;

public class TransferItem
{
    public required string SourcePath { get; set; }
    public required TransferItemStatus Status { get; set; }
    public required bool IsRenamed { get; set; }
    public required long Size { get; set; }
    public string? FileId { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int TotalPartsCount { get; set; }

    // A file just over MinMultipartSizeBytes takes the multipart path but produces a single part,
    // so the part count alone cannot distinguish it from a single upload.
    public bool IsMultipart { get; set; }
}
