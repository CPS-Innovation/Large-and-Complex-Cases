using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Egress.Models.Response;


public class ListWorkspacesResponse
{
  [JsonPropertyName("data")]
  public required IEnumerable<ListWorkspacesResponseData> Data { get; set; }
  [JsonPropertyName("data_info")]
  public required DataInfoResponse DataInfo { get; set; }
}

public class ListWorkspacesResponseData
{
  [JsonPropertyName("id")]
  public required string Id { get; set; }
  [JsonPropertyName("name")]
  public required string Name { get; set; }
  [JsonPropertyName("date_created")]
  public string? DateCreated { get; set; }
}
