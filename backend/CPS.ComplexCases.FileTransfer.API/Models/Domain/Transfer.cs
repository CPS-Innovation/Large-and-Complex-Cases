using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;

namespace CPS.ComplexCases.FileTransfer.API.Models.Domain;

public class Transfer
{
    public Guid Id { get; set; }
    public int CaseId { get; set; }
    public TransferStatus Status { get; set; }
    public TransferType TransferType { get; set; }
    public TransferDirection Direction { get; set; }
    public required string SourcePath { get; set; }
    public required string DestinationPath { get; set; }
    public int TotalFiles { get; set; }
    public int ProcessedFiles { get; set; }
    public int SuccessfulFiles { get; set; }
    public int FailedFiles { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int RetryCount { get; set; }
    public DateTime? LastRetryAt { get; set; }
    public Guid? ParentTransferId { get; set; }
    public virtual ICollection<TransferItem> Items { get; set; } = new List<TransferItem>();
}