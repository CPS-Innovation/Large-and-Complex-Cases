using System.Text.Json.Serialization;

namespace CPS.ComplexCases.DDEI.Models.Dto;

public class AreaDto
{
  [JsonPropertyName("id")]
  public int Id { get; set; }
  [JsonPropertyName("description")]
  public required string Description { get; set; }
}