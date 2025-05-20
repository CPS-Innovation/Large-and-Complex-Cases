using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;

namespace CPS.ComplexCases.FileTransfer.API.Models.Domain;

public class TransferItemPart
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public int PartNumber { get; set; }
    public string? ETag { get; set; }
    public long Bytes { get; set; }
    public PartStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public required virtual TransferItem ParentItem { get; set; }
}