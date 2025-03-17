using System.Text.Json.Serialization;

namespace CPS.ComplexCases.DDEI.Models.Response;

public class DdeiUnitDto
{
  [JsonPropertyName("id")]
  public int Id { get; set; }
  [JsonPropertyName("description")]
  public required string Description { get; set; }
  [JsonPropertyName("areaId")]
  public int AreaId { get; set; }
  [JsonPropertyName("areaDescription")]
  public required string AreaDescription { get; set; }
}
