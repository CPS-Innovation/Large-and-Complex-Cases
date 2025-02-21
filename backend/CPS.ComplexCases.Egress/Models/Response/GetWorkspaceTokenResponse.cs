using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Egress.Models.Response;

public class GetWorkspaceTokenResponse
{
  [JsonPropertyName("token")]
  public required string Token { get; set; }
  [JsonPropertyName("duration")]
  public int? Expiration { get; set; }
}
