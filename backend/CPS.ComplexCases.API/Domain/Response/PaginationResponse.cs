using System.Text.Json.Serialization;

namespace CPS.ComplexCases.API.Domain.Response;

public class PaginationResponse
{
  [JsonPropertyName("count")]
  public int Count { get; set; }
  [JsonPropertyName("take")]
  public int Take { get; set; }
  [JsonPropertyName("skip")]
  public int Skip { get; set; }
  [JsonPropertyName("totalResults")]
  public int TotalResults { get; set; }
}
