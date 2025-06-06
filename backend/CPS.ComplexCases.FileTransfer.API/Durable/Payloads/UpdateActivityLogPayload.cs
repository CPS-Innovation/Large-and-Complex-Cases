using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Payloads;

public class UpdateActivityLogPayload
{
    public required ActivityLog.Enums.ActionType ActionType { get; set; }
    public required Guid CaseId { get; set; }
    public required string TransferId { get; set; }
    public required TransferDirection TransferDirection { get; set; }
    public string? UserName { get; set; }
    public List<TransferItem> SuccessfulItems { get; set; } = [];
    public List<TransferFailedItem> FailedItems { get; set; } = [];
}