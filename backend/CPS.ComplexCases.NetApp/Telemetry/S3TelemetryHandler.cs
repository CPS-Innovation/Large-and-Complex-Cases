using Amazon.Runtime;
using CPS.ComplexCases.Common.Telemetry;

namespace CPS.ComplexCases.NetApp.Telemetry;

public class S3TelemetryHandler(ITelemetryClient telemetryClient) : IS3TelemetryHandler
{
    private readonly ITelemetryClient _telemetryClient = telemetryClient;
    private ExternalApiCallEvent? _telemetryEvent;

    public void InitiateTelemetryEvent(WebServiceRequestEventArgs? args)
    {
        if (args == null) return;
        _telemetryEvent = new ExternalApiCallEvent(args.ServiceName, "AmazonS3Client Request")
        {
            RequestUri = args.Endpoint.AbsoluteUri,
            Operation = args.Request.GetType().Name
        };
    }

    public void CompleteTelemetryEvent(WebServiceResponseEventArgs? args)
    {
        if (args == null) return;
        if (_telemetryEvent != null)
        {
            _telemetryEvent.ResponseStatusCode = args.Response.HttpStatusCode;
            _telemetryEvent.CallEndTime = DateTime.UtcNow;
            _telemetryClient.TrackEvent(_telemetryEvent);
        }
    }
}
