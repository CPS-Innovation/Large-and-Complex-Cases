using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace CPS.ComplexCases.Common.Telemetry;

public class HealthCheckTelemetryFilter(ITelemetryProcessor next) : ITelemetryProcessor
{
    private static readonly string[] PeriodicHealthPaths =
    [
        "/api/status",
        "/api/health"
    ];

    private const string StatusFunctionName = "Status";

    private readonly ITelemetryProcessor _next = next;

    public void Process(ITelemetry item)
    {
        if (ShouldFilter(item))
        {
            return;
        }

        _next.Process(item);
    }

    private static bool ShouldFilter(ITelemetry item)
    {
        if (IsPeriodicHealthOperation(item.Context.Operation.Name))
        {
            return true;
        }

        if (item is ISupportProperties props)
        {
            if (props.Properties.TryGetValue("AzureFunctions_FunctionName", out var functionName) &&
                IsPeriodicHealthFunctionName(functionName))
            {
                return true;
            }

            if (props.Properties.TryGetValue("RequestPath", out var path) &&
                IsPeriodicHealthPath(path))
            {
                return true;
            }
        }

        if (item is RequestTelemetry request)
        {
            return IsPeriodicHealthOperation(request.Name)
                || IsPeriodicHealthPath(request.Url?.AbsolutePath)
                || IsPeriodicHealthProbeValue(request.Url?.ToString());
        }

        if (item is DependencyTelemetry dependency)
        {
            return IsPeriodicHealthProbeValue(dependency.Data)
                || IsPeriodicHealthProbeValue(dependency.Name)
                || IsPeriodicHealthProbeValue(dependency.Target);
        }

        return false;
    }

    private static bool IsPeriodicHealthFunctionName(string? name) =>
        string.Equals(name, StatusFunctionName, StringComparison.OrdinalIgnoreCase);

    private static bool IsPeriodicHealthOperation(string? name) =>
        IsPeriodicHealthFunctionName(name) || IsPeriodicHealthProbeValue(name);

    private static bool IsPeriodicHealthPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var normalized = path.Trim().TrimEnd('/');
        foreach (var healthPath in PeriodicHealthPaths)
        {
            if (string.Equals(normalized, healthPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsPeriodicHealthProbeValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim();
        if (IsPeriodicHealthPath(candidate))
        {
            return true;
        }

        var spaceIndex = candidate.IndexOf(' ');
        if (spaceIndex > 0 && spaceIndex < candidate.Length - 1)
        {
            var maybeMethod = candidate[..spaceIndex];
            if (maybeMethod.Length <= 10 && maybeMethod.All(char.IsLetter))
            {
                candidate = candidate[(spaceIndex + 1)..].Trim();
                if (IsPeriodicHealthPath(candidate))
                {
                    return true;
                }
            }
        }

        if (Uri.TryCreate(candidate, UriKind.Absolute, out var uri))
        {
            return IsPeriodicHealthPath(uri.AbsolutePath);
        }

        var pathPart = candidate.Split('?', 2)[0];
        return IsPeriodicHealthPath(pathPart);
    }
}
