using System.Text.Json.Serialization;
using CPS.ComplexCases.API.Constants;

namespace CPS.ComplexCases.API.Domain.Models;

public class UiTelemetry
{
    [JsonPropertyName("telemetryType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TelemetryType TelemetryType { get; set; }
    [JsonPropertyName("eventTimestamp")]
    public DateTime EventTimestamp { get; set; }
    [JsonPropertyName("properties")]
    public List<Dictionary<string, object>>? Properties { get; set; } = [];
}