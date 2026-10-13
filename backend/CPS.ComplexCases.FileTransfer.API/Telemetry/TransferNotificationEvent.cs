using CPS.ComplexCases.Common.Telemetry;

namespace CPS.ComplexCases.FileTransfer.API.Telemetry;

public class TransferNotificationEvent : BaseTelemetryEvent
{
    public const string StatusSent = "Sent";
    public const string StatusFailed = "Failed";
    public const string StatusSkipped = "Skipped";

    public Guid TransferId { get; set; }
    public string? TemplateId { get; set; }
    public string? NotificationId { get; set; }
    public string? Recipient { get; set; }
    public required string DispatchStatus { get; set; }
    public string? FailureReason { get; set; }

    public override (IDictionary<string, string> Properties, IDictionary<string, double?> Metrics) ToTelemetryEventProps()
    {
        var properties = new Dictionary<string, string>
        {
            { nameof(CaseId), CaseId.ToString() },
            { nameof(TransferId), TransferId.ToString() },
            { nameof(CorrelationId), CorrelationId.ToString() },
            { nameof(DispatchStatus), DispatchStatus },
        };

        if (!string.IsNullOrEmpty(TemplateId))
        {
            properties[nameof(TemplateId)] = TemplateId;
        }

        if (!string.IsNullOrEmpty(NotificationId))
        {
            properties[nameof(NotificationId)] = NotificationId;
        }

        if (!string.IsNullOrEmpty(Recipient))
        {
            properties[nameof(Recipient)] = Recipient;
        }

        if (!string.IsNullOrEmpty(FailureReason))
        {
            properties[nameof(FailureReason)] = FailureReason;
        }

        return (properties, new Dictionary<string, double?>());
    }
}
