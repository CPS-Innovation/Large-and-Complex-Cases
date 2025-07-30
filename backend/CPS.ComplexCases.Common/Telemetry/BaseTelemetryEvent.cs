namespace CPS.ComplexCases.Common.Telemetry;

public abstract class BaseTelemetryEvent
{
    public string EventName
    {
        get
        {
            return GetType().Name;
        }
    }

    abstract public (IDictionary<string, string>, IDictionary<string, double?>) ToTelemetryEventProps();
}