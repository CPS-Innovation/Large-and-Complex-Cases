using CPS.ComplexCases.ActivityLog.Enums;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Payloads;

public class UpdateActivityLogPayload
{
    public required string TransferId { get; set; }
    public required ActionType ActionType { get; set; }
    public string? UserName { get; set; }
    public string? ExceptionMessage { get; set; }
}