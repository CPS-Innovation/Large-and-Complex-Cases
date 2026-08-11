using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.DataContracts;

namespace CPS.ComplexCases.Common.Telemetry;

public class HealthCheckTelemetryFilter(ITelemetryProcessor next) : ITelemetryProcessor
{
    private readonly ITelemetryProcessor _next = next;

    public void Process(ITelemetry item)
    {
        if (item is ISupportProperties props)
        {
            if (props.Properties.TryGetValue(
                    "AzureFunctions_FunctionName",
                    out var functionName) &&
                string.Equals(
                    functionName,
                    "Status",
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (props.Properties.TryGetValue(
                    "RequestPath",
                    out var path) &&
                string.Equals(
                    path,
                    "/api/status",
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        _next.Process(item);
    }
}