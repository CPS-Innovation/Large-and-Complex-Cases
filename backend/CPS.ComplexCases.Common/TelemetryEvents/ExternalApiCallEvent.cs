using System.Net;

namespace CPS.ComplexCases.Common.Telemetry;

public class ExternalApiCallEvent : BaseTelemetryEvent
{
    public ExternalApiCallEvent(string apiName, HttpRequestMessage request, string operation)
    {
        ApiName = apiName;
        RequestUri = request.RequestUri?.ToString() ?? string.Empty;
        HttpMethod = request.Method.Method;
        Operation = operation;
        CallStartTime = DateTime.UtcNow;
    }

    public string ApiName { get; set; }
    public string RequestUri { get; set; }
    public string HttpMethod { get; set; }
    public string Operation { get; set; }
    public HttpStatusCode? ResponseStatusCode { get; set; }
    public DateTime CallStartTime { get; set; }
    public DateTime CallEndTime { get; set; }

    public override (IDictionary<string, string> Properties, IDictionary<string, double?> Metrics) ToTelemetryEventProps()
    {
        return (new Dictionary<string, string>
        {
            { nameof(CaseId), CaseId.ToString() },
            { nameof(ApiName), ApiName },
            { nameof(RequestUri), RequestUri },
            { nameof(HttpMethod), HttpMethod },
            { nameof(Operation), Operation },
            { nameof(ResponseStatusCode), ResponseStatusCode.HasValue ? ((int)ResponseStatusCode.Value).ToString() : "N/A" }
        }, new Dictionary<string, double?>
        {
            { TelemetryConstants.DurationCustomDimensionName, GetDurationInMilliseconds(CallStartTime, CallEndTime) },
        });
    }
}