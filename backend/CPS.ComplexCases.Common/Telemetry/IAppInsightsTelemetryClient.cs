using Microsoft.ApplicationInsights.DataContracts;

namespace CPS.ComplexCases.Common.Telemetry;

public interface IAppInsightsTelemetryClient
{
    void TrackEvent(string eventName, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null);
    void TrackException(Exception exception, IDictionary<string, string>? properties = null, IDictionary<string, double>? metrics = null);
    void TrackMetric(string name, double value, IDictionary<string, string>? properties = null);
    void TrackPageView(string name);
    void TrackTrace(string message, SeverityLevel severityLevel, IDictionary<string, string>? properties = null);
}