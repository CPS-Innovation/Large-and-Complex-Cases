using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Egress.Models.Args;

public class PaginationArg
{
  [JsonPropertyName("skip")]
  public int Skip { get; set; }
  [JsonPropertyName("take")]
  public int Take { get; set; }
}