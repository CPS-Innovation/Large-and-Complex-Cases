using Amazon.Runtime;

namespace CPS.ComplexCases.NetApp.Telemetry;

public interface IS3TelemetryHandler
{
    void InitiateTelemetryEvent(WebServiceRequestEventArgs? args);
    void CompleteTelemetryEvent(WebServiceResponseEventArgs? args);
}