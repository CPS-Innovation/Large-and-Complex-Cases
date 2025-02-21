using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Egress.Models.Response;


public class FindWorkspaceResponse
{
  [JsonPropertyName("data")]
  public required IEnumerable<FindWorkspaceResponseData> Data { get; set; }
  [JsonPropertyName("pagination")]
  public required PaginationResponse Pagination { get; set; }
}

public class FindWorkspaceResponseData
{
  [JsonPropertyName("id")]
  public required string Id { get; set; }
  [JsonPropertyName("_links")]
  public required WorkspaceLinks Links { get; set; }
  [JsonPropertyName("name")]
  public required string Name { get; set; }
}

public class WorkspaceLinks
{
  [JsonPropertyName("properties")]
  public required string Properties { get; set; }
}
