using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Egress.Models.Response;


public class GetWorkspacePermissionsResponse
{
  [JsonPropertyName("data")]
  public required IEnumerable<GetWorkspacePersmissionsResponseData> Data { get; set; }
  [JsonPropertyName("pagination")]
  public required PaginationResponse Pagination { get; set; }
}

public class GetWorkspacePersmissionsResponseData
{
  [JsonPropertyName("switch_id")]
  public required string Email { get; set; }
  [JsonPropertyName("role_id")]
  public required string RoleId { get; set; }
}