namespace CPS.ComplexCases.Common.Telemetry;

public interface ITelemetryClient
{
    void TrackEvent(BaseTelemetryEvent telemetryEvent);
    void TrackEventFailure(BaseTelemetryEvent telemetryEvent);
    void TrackException(BaseTelemetryEvent telemetryEvent);
    void TrackMetric(BaseTelemetryEvent telemetryEvent);
    void TrackPageView(BaseTelemetryEvent telemetryEvent);
    void TrackTrace(BaseTelemetryEvent telemetryEvent);
}