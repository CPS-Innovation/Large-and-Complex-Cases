using System.Text.Json.Serialization;

namespace CPS.ComplexCases.DDEI.WireMock.Models;

public class DdeiUserFilteredDataDto
{
  [JsonPropertyName("areas")]
  public required List<DdeiAreaDto> Areas { get; set; }
}

public class DdeiAreaDto
{
  [JsonPropertyName("id")]
  public int Id { get; set; }
  [JsonPropertyName("description")]
  public required string Description { get; set; }
}