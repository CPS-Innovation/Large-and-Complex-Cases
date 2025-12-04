namespace CPS.ComplexCases.API.Telemetry;

public abstract class BaseTelemetryEvent
{
    public string EventName
    {
        get
        {
            return this.GetType().Name;
        }
    }

    public Guid CorrelationId { get; set; }

    public DateTime EventTimestamp { get; set; }

    abstract public (IDictionary<string, string> Properties, IDictionary<string, double?> Metrics) ToTelemetryEventProps();
}