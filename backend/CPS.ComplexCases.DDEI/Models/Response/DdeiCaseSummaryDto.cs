using System.Text.Json.Serialization;

namespace CPS.ComplexCases.DDEI.Models.Response;

public class DdeiCaseSummaryDto
{
  [JsonPropertyName("urn")]
  public required string Urn { get; set; }
  [JsonPropertyName("id")]
  public int Id { get; set; }
  [JsonPropertyName("leadDefendantFirstNames")]
  public string? LeadDefendantFirstNames { get; set; }
  [JsonPropertyName("leadDefendantSurname")]
  public string? LeadDefendantSurname { get; set; }
  [JsonPropertyName("operation")]
  public string? Operation { get; set; }
}