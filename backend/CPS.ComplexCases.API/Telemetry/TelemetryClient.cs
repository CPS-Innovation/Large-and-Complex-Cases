using Microsoft.ApplicationInsights.DataContracts;

namespace CPS.ComplexCases.API.Telemetry;

public class TelemetryClient(IAppInsightsTelemetryClient telemetryClient) : ITelemetryClient
{
    protected readonly IAppInsightsTelemetryClient _telemetryClient = telemetryClient;

    private const string ExceptionMessageKey = "exceptionMessage";
    private const string PageNameKey = "pageName";
    private const string SeverityLevelKey = "severityLevel";
    private const string MessageKey = "message";

    public void TrackEvent(BaseTelemetryEvent telemetryEvent)
    {
        TrackInternalEvent(telemetryEvent, isFailure: false);
    }

    public void TrackEventFailure(BaseTelemetryEvent telemetryEvent)
    {
        TrackInternalEvent(telemetryEvent, isFailure: true);
    }

    public void TrackException(BaseTelemetryEvent telemetryEvent)
    {
        if (telemetryEvent == null)
            return;

        var (properties, metrics) = PrepareTelemetryEventProps(telemetryEvent, true);

        var exceptionMessage = "Exception occurred";
        if (properties != null && properties.ContainsKey(ExceptionMessageKey))
        {
            exceptionMessage = properties[ExceptionMessageKey] ?? "Exception occurred";
        }

        var exception = new Exception(exceptionMessage);

        _telemetryClient.TrackException(exception, properties, metrics);
    }

    public void TrackMetric(BaseTelemetryEvent telemetryEvent)
    {
        if (telemetryEvent == null)
            return;

        var (properties, metrics) = PrepareTelemetryEventProps(telemetryEvent);

        if (metrics == null)
            return;

        foreach (var metric in metrics)
        {
            _telemetryClient.TrackMetric(
                metric.Key,
                metric.Value,
                properties);
        }
    }

    public void TrackPageView(BaseTelemetryEvent telemetryEvent)
    {
        if (telemetryEvent == null)
            return;

        var (properties, metrics) = PrepareTelemetryEventProps(telemetryEvent);

        if (!properties.ContainsKey(PageNameKey))
            return;

        var pageName = properties?[PageNameKey]?.ToString() ?? string.Empty;

        if (string.IsNullOrEmpty(pageName))
            return;

        _telemetryClient.TrackPageView(pageName);
    }

    public void TrackTrace(BaseTelemetryEvent telemetryEvent)
    {
        if (telemetryEvent == null)
            return;

        var (properties, metrics) = PrepareTelemetryEventProps(telemetryEvent);

        var message = string.Empty;
        if (properties != null && properties.ContainsKey(MessageKey))
        {
            message = properties[MessageKey] ?? string.Empty;
        }

        var severityLevel = SeverityLevel.Information.ToString();
        if (properties != null && properties.ContainsKey(SeverityLevelKey))
        {
            severityLevel = properties[SeverityLevelKey]?.ToString() ?? SeverityLevel.Information.ToString();
        }

        TryParseEnum<SeverityLevel>(severityLevel, out var parsedSeverityLevel);
        var appInsightsSeverityLevel = parsedSeverityLevel ?? SeverityLevel.Information;

        _telemetryClient.TrackTrace(message,
            appInsightsSeverityLevel,
            properties);
    }

    private void TrackInternalEvent(BaseTelemetryEvent telemetryEvent, bool isFailure = false)
    {
        if (telemetryEvent == null)
            return;

        var (properties, metrics) = PrepareTelemetryEventProps(telemetryEvent, isFailure);

        _telemetryClient.TrackEvent(
            PrepareEventName(telemetryEvent.EventName),
            properties,
            metrics
        );
    }

    private (IDictionary<string, string> Properties, IDictionary<string, double>? Metrics) PrepareTelemetryEventProps(BaseTelemetryEvent telemetryEvent, bool isFailure = false)
    {
        var (properties, metrics) = telemetryEvent.ToTelemetryEventProps();

        var nonNullMetrics = metrics.Where(kvp => kvp.Value.HasValue)
                                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value!.Value);

        if (!telemetryEvent.CorrelationId.Equals(Guid.Empty))
        {
            properties["correlationId"] = telemetryEvent.CorrelationId.ToString();
        }

        if (telemetryEvent.EventTimestamp != DateTime.MinValue)
        {
            properties["eventTimestamp"] = telemetryEvent.EventTimestamp.ToString("dd/MM/yyyy HH:mm:ss.fff");
        }

        if (isFailure)
        {
            properties["isFailure"] = "true";
        }

        return (PrepareKeyNames(properties), PrepareKeyNames(nonNullMetrics));
    }

    private static string PrepareEventName(string eventName)
    {
        if (!eventName.EndsWith("Event"))
        {
            return eventName;
        }

        return eventName.Remove(eventName.LastIndexOf("Event"));
    }

    private static IDictionary<string, T> PrepareKeyNames<T>(IDictionary<string, T>? properties)
    {
        var cleanedProperties = new Dictionary<string, T>();

        if (properties == null)
        {
            return cleanedProperties;
        }

        foreach (var property in properties)
        {
            cleanedProperties[CleanPropertyName(property.Key)] = property.Value;
        }

        return cleanedProperties;
    }

    private static string CleanPropertyName(string name)
    {
        var propertyName = name.Replace("_", string.Empty);

        return ToLowerFirstChar(propertyName);
    }

    public static string ToLowerFirstChar(string input)
    {
        if (string.IsNullOrEmpty(input) || char.IsLower(input, 0))
        {
            return input;
        }

        return char.ToLower(input[0]) + input.Substring(1);
    }

    public static bool TryParseEnum<TEnum>(string value, out TEnum? result) where TEnum : struct, Enum
    {
        result = null;

        if (Enum.TryParse(value, out TEnum parsedEnum))
        {
            result = parsedEnum;
            return true;
        }

        return false;
    }
}