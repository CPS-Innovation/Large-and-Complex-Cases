using System.Text.Json.Serialization;

namespace CPS.ComplexCases.DDEI.Models.Response;

public class DdeiCaseIdentifiersDto
{
  [JsonPropertyName("id")]
  public int Id { get; set; }
  [JsonPropertyName("urn")]
  public required string Urn { get; set; }
}