using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace CPS.ComplexCases.FileTransfer.API.Telemetry;

public class AzureSdkTraceFilter(ITelemetryProcessor next) : ITelemetryProcessor
{
    private readonly ITelemetryProcessor _next = next;

    public void Process(ITelemetry item)
    {
        if (item is TraceTelemetry trace
            && trace.Message != null
            && trace.Message.Contains("client assembly: Azure."))
        {
            return;
        }

        _next.Process(item);
    }
}
