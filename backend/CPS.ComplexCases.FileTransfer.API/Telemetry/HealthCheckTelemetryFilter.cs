using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.DataContracts;

namespace CPS.ComplexCases.FileTransfer.API.Telemetry;

public class HealthCheckTelemetryFilter(ITelemetryProcessor next) : ITelemetryProcessor
{
    private readonly ITelemetryProcessor _next = next;

    public void Process(ITelemetry item)
    {
        if (item is ISupportProperties props)
        {
            if (props.Properties.TryGetValue("RequestPath", out var path) &&
                path.Equals("/api/status", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (props.Properties.TryGetValue("AzureFunctions_FunctionName", out var functionName) &&
                functionName.Equals("Status", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
        }

        _next.Process(item);
    }
}