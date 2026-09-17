using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;

// Load the host extension so HealthCheckTelemetryFilter can drop /api/status telemetry
// emitted by the Functions host (worker-side processors never see those items).
[assembly: ExtensionInformation("CPS.ComplexCases.API.HttpTelemetry", "1.0.42")]
