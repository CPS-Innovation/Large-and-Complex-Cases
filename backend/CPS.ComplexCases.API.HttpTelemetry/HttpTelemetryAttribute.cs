namespace CPS.ComplexCases.API.HttpTelemetry;

[AttributeUsage(AttributeTargets.Parameter)]
[Microsoft.Azure.WebJobs.Description.Binding]
public class HttpTelemetryAttribute : Attribute
{
}