using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Egress.Models.Response;


public class ListEgressTemplatesResponse
{
    [JsonPropertyName("data")]
    public required IEnumerable<ListEgressTemplatesResponseData> Data { get; set; }
    [JsonPropertyName("data_info")]
    public required DataInfoResponse DataInfo { get; set; }
}

public class ListEgressTemplatesResponseData
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    [JsonPropertyName("name")]
    public required string Name { get; set; }
    [JsonPropertyName("description")]
    public string? Description { get; set; }
    [JsonPropertyName("priority")]
    public int? Priority { get; set; }
}
