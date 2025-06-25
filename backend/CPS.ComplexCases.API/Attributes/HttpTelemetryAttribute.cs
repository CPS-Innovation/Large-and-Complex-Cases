using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;

namespace CPS.ComplexCases.API.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class HttpTelemetryAttribute : InputBindingAttribute
    {
    }
}