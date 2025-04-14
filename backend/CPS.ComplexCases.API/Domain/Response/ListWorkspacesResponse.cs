using System.Text.Json.Serialization;

namespace CPS.ComplexCases.API.Domain.Response;

public class ListWorkspacesResponse
{
  public required IEnumerable<ListWorkspaceDataResponse> Data { get; set; }
  public required PaginationResponse Pagination { get; set; }
}


public class ListWorkspaceDataResponse
{
  [JsonPropertyName("id")]
  public required string Id { get; set; }
  [JsonPropertyName("name")]
  public required string Name { get; set; }
  [JsonPropertyName("dateCreated")]
  public string? DateCreated { get; set; }
  [JsonPropertyName("caseId")]
  public int? CaseId { get; set; }
}