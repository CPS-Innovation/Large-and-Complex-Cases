using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Egress.Models.Response;

public class DataInfoResponse
{
  [JsonPropertyName("num_returned")]
  public int NumReturned { get; set; }
  [JsonPropertyName("skip")]
  public int Skip { get; set; }
  [JsonPropertyName("limit")]
  public int Limit { get; set; }
  [JsonPropertyName("total_results")]
  public int TotalResults { get; set; }
}
