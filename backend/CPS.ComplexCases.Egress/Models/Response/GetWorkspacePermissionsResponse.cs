using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Egress.Models.Response;


public class GetWorkspacePermissionsResponse
{
  [JsonPropertyName("data")]
  public required IEnumerable<GetWorkspacePersmissionsResponseData> Data { get; set; }
  [JsonPropertyName("data_info")]
  public required DataInfoResponse DataInfo { get; set; }
}

public class GetWorkspacePersmissionsResponseData
{
  [JsonPropertyName("switch_id")]
  public required string Email { get; set; }
  [JsonPropertyName("role_id")]
  public required string RoleId { get; set; }
}