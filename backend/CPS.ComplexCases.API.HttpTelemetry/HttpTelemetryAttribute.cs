using Microsoft.Azure.WebJobs.Description;

namespace CPS.ComplexCases.API.HttpTelemetry;

[AttributeUsage(AttributeTargets.Parameter)]
[Binding]
public class HttpTelemetryAttribute : Attribute
{
}