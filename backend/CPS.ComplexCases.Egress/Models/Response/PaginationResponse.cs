using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Egress.Models.Response;

public class PaginationResponse
{
  [JsonPropertyName("current_page_num")]
  public int CurrentPage { get; set; }
  [JsonPropertyName("per_page")]
  public int PerPage { get; set; }
  [JsonPropertyName("total_pages")]
  public int TotalPages { get; set; }
  [JsonPropertyName("total_results")]
  public int TotalResults { get; set; }
}
