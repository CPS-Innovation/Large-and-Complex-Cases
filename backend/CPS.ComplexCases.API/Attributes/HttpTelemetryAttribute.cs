using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;

[assembly: ExtensionInformation("CPS.ComplexCases.API.HttpTelemetry", "1.0.41")]
namespace CPS.ComplexCases.API.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class HttpTelemetryAttribute : InputBindingAttribute
    {
    }
}