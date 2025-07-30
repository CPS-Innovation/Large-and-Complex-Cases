using CPS.ComplexCases.Common.Telemetry;

namespace CPS.ComplexCases.FileTransfer.API.TelemetryEvents;

public class BaseTransferEvent : BaseTelemetryEvent
{
    public Guid CorrelationId { get; set; }
    public long CaseId { get; set; }
    public string? FileName { get; set; }

    public override (IDictionary<string, string>, IDictionary<string, double?>) ToTelemetryEventProps()
    {
        var properties = new Dictionary<string, string>
        {
            { nameof(CorrelationId), CorrelationId.ToString() },
            { nameof(CaseId), CaseId.ToString() },
            { nameof(FileName), FileName ?? string.Empty }
        };

        var metrics = new Dictionary<string, double?>();

        return (properties, metrics);
    }
}