namespace CPS.ComplexCases.Common.Telemetry;

public class TelemetryClientStub : ITelemetryClient
{
    public void TrackEvent(BaseTelemetryEvent telemetryEvent)
    {
    }

    public void TrackEventFailure(BaseTelemetryEvent telemetryEvent)
    {
    }

    public void TrackException(BaseTelemetryEvent telemetryEvent)
    {
    }

    public void TrackMetric(BaseTelemetryEvent telemetryEvent)
    {
    }

    public void TrackPageView(BaseTelemetryEvent telemetryEvent)
    {
    }

    public void TrackTrace(BaseTelemetryEvent telemetryEvent)
    {
        throw new NotImplementedException();
    }
}