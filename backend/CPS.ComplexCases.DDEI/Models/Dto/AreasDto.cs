using System.Text.Json.Serialization;

namespace CPS.ComplexCases.DDEI.Models.Dto;

public class AreasDto
{
  [JsonPropertyName("allAreas")]
  public List<AreaDto> AllAreas { get; set; } = [];
  [JsonPropertyName("userAreas")]
  public List<AreaDto> UserAreas { get; set; } = [];
  [JsonPropertyName("homeArea")]
  public AreaDto? HomeArea { get; set; }
}