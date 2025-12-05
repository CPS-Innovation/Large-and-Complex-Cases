namespace CPS.ComplexCases.Common.Telemetry;

using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;

public class TelemetryInitializer(IConfiguration configuration) : ITelemetryInitializer
{
    public const string Version = "0.1";
    private readonly IConfiguration _configuration = configuration;

    public void Initialize(ITelemetry telemetry)
    {
        telemetry.Context.GlobalProperties["telemetryVersion"] = Version;
        telemetry.Context.GlobalProperties["environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown";
        telemetry.Context.GlobalProperties["appName"] = _configuration["AppName"] ?? "Cps.ComplexCases";
    }
}