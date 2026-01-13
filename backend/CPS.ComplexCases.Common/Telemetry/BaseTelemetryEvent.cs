namespace CPS.ComplexCases.Common.Telemetry;

public abstract class BaseTelemetryEvent
{
    public string EventName
    {
        get
        {
            return this.GetType().Name;
        }
    }

    public int CaseId { get; set; }

    public Guid CorrelationId { get; set; }

    public DateTime EventTimestamp { get; set; }

    abstract public (IDictionary<string, string> Properties, IDictionary<string, double?> Metrics) ToTelemetryEventProps();

    public static double? GetDurationInMilliseconds(DateTime startTime, DateTime endTime)
    {
        if (startTime == default || endTime == default || endTime < startTime)
            return null;

        return (endTime - startTime).TotalMilliseconds;
    }
}