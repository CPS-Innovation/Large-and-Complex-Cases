using System.Text.Json.Serialization;

namespace CPS.ComplexCases.DDEI.WireMock.Models;

public class DdeiCaseIdentifiersDto
{
  [JsonPropertyName("id")]
  public int Id { get; set; }
  [JsonPropertyName("urn")]
  public required string Urn { get; set; }
}