using AppInsights = Microsoft.ApplicationInsights;

namespace CPS.ComplexCases.Common.Telemetry;

public class TelemetryClient(AppInsights.TelemetryClient telemetryClient) : ITelemetryClient
{
    protected readonly AppInsights.TelemetryClient _telemetryClient = telemetryClient ?? throw new ArgumentNullException(nameof(telemetryClient), "Telemetry client cannot be null.");

    public void TrackEvent(BaseTelemetryEvent baseTelemetryEvent)
    {
        TrackEventInternal(baseTelemetryEvent, false);
    }

    public void TrackEventFailure(BaseTelemetryEvent baseTelemetryEvent)
    {
        TrackEventInternal(baseTelemetryEvent, true);
    }

    private void TrackEventInternal(BaseTelemetryEvent baseTelemetryEvent, bool isFailure)
    {
        if (baseTelemetryEvent == null)
        {
            return;
        }

        var (properties, metrics) = baseTelemetryEvent.ToTelemetryEventProps();

        var nonNullMetrics = metrics.Where(kvp => kvp.Value.HasValue).ToDictionary(kvp => kvp.Key, kvp => (double)kvp.Value!);

        if (isFailure)
        {
            properties.Add("IsFailure", "true");
        }

        _telemetryClient.TrackEvent(
            PrepareEventName(baseTelemetryEvent.EventName),
            PrepareKeyNames(properties),
            PrepareKeyNames(nonNullMetrics));
    }

    private static string PrepareEventName(string eventName)
    {
        if (!eventName.EndsWith("Event"))
            return eventName;

        return eventName.Remove(eventName.LastIndexOf("Event"));
    }

    private static Dictionary<string, T> PrepareKeyNames<T>(IDictionary<string, T> properties)
    {
        var cleanedProperties = new Dictionary<string, T>();

        foreach (var property in properties)
        {
            cleanedProperties.Add(CleanPropertyName(property.Key), property.Value);
        }

        return cleanedProperties;
    }

    private static string CleanPropertyName(string name)
    {
        var propertyName = name.Replace("_", string.Empty);
        return ToLowerFirstCase(propertyName);
    }

    private static string ToLowerFirstCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return char.ToLowerInvariant(input[0]) + input.Substring(1);
    }
}