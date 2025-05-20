using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;

namespace CPS.ComplexCases.FileTransfer.API.Models.Domain;

public class TransferItem
{
    public Guid Id { get; set; }
    public Guid TransferId { get; set; }
    public required string SourcePath { get; set; }
    public required string DestinationPath { get; set; }
    public TransferStatus Status { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public long BytesTotal { get; set; }
    public long BytesTransferred { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int RetryCount { get; set; }
    public required virtual Transfer ParentTransfer { get; set; }
    public virtual ICollection<TransferItemPart> Parts { get; set; } = new List<TransferItemPart>();
}