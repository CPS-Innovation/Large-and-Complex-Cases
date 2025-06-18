using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;

public class TransferEntity
{
    public Guid Id { get; set; }
    public int CaseId { get; set; }
    public TransferStatus Status { get; set; }
    public TransferType TransferType { get; set; }
    public TransferDirection Direction { get; set; }
    public List<TransferSourcePath> SourcePaths { get; set; } = new List<TransferSourcePath>();
    public required string DestinationPath { get; set; }
    public int TotalFiles { get; set; }
    public int ProcessedFiles { get; set; }
    public int SuccessfulFiles { get; set; }
    public int FailedFiles { get; set; }
    public DateTime? StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<TransferItem> SuccessfulItems { get; set; } = new List<TransferItem>();
    public List<TransferFailedItem> FailedItems { get; set; } = new List<TransferFailedItem>();
    public bool? IsRetry { get; set; } = false;
    public string? UserName { get; set; }
    public Guid? CorrelationId { get; set; } = null;
}
