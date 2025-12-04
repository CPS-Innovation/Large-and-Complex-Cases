namespace CPS.ComplexCases.API.Telemetry;

public class BaseUiTelemetryEvent : BaseTelemetryEvent
{
    public IEnumerable<Dictionary<string, object>> Properties { get; set; } = new List<Dictionary<string, object>>();

    public override (IDictionary<string, string> Properties, IDictionary<string, double?> Metrics) ToTelemetryEventProps()
    {
        var telemetryProperties = Properties.SelectMany(dict => dict)
                                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        if (telemetryProperties == null || telemetryProperties.Count == 0)
        {
            return (new Dictionary<string, string>(), new Dictionary<string, double?>());
        }

        var properties = new Dictionary<string, string>();
        var metrics = new Dictionary<string, double?>();

        foreach (var kvp in telemetryProperties)
        {
            if (kvp.Value is string stringValue)
            {
                properties.Add(kvp.Key, stringValue);
            }
            else if (kvp.Value is int intValue)
            {
                metrics.Add(kvp.Key, intValue);
            }
            else if (kvp.Value is double doubleValue)
            {
                metrics.Add(kvp.Key, doubleValue);
            }
            else if (kvp.Value is float floatValue)
            {
                metrics.Add(kvp.Key, floatValue);
            }
            else if (kvp.Value is long longValue)
            {
                metrics.Add(kvp.Key, longValue);
            }
            else if (kvp.Value is decimal decimalValue)
            {
                metrics.Add(kvp.Key, (double)decimalValue);
            }
            else
            {
                properties.Add(kvp.Key, kvp.Value?.ToString() ?? string.Empty);
            }
        }

        return (properties, metrics);
    }
}