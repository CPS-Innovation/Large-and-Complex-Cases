
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace CPS.ComplexCases.API.HttpTelemetry;

public class HttpTelemetryInitializer : ITelemetryInitializer
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpTelemetryInitializer(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Initialize(ITelemetry telemetry)
    {
        if (telemetry is RequestTelemetry rt)
        {
            try
            {
                var headers = _httpContextAccessor?.HttpContext?.Request?.Headers;
                if (headers == null)
                {
                    return;
                }

                var correlationId = GetHeaderValue(headers, Constants.CorrelationIdHeaderName);
                if (!string.IsNullOrWhiteSpace(correlationId))
                {
                    rt.Properties.Add(Constants.CorrelationIdHeaderName, correlationId);
                }

                var token = GetHeaderValue(headers, Constants.AuthorizationHeaderName);
                if (token != null)
                {
                    var tokenObject = JWTDecoder.Decoder.DecodePayload<Token>(token.Split(" ").Last());
                    rt.Properties.Add(Constants.UsernameAnalyticsPropertyName, tokenObject?.Username);
                }
            }
            catch (Exception exception)
            {
                rt.Properties.Add(Constants.HttpTelemetryInitializerError, exception.ToString());
            }
        }
    }

    private class Token
    {
        [JsonProperty(Constants.PreferredUsername)]
        public string? Username { get; set; }
    }

    private static string? GetHeaderValue(IHeaderDictionary headers, string headerName)
    {
        if (headers.TryGetValue(headerName, out var values))
        {
            return values.ToString();
        }

        return null;
    }
}