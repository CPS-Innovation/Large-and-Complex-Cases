using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;

[assembly: ExtensionInformation("CPS.ComplexCases.API.HttpTelemetry", "1.0.41")]

namespace CPS.ComplexCases.API.HttpTelemetry;

[AttributeUsage(AttributeTargets.Parameter)]
[Microsoft.Azure.WebJobs.Description.Binding]
public class HttpTelemetryAttribute : Attribute
{
}