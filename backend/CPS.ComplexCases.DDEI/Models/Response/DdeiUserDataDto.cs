using System.Text.Json.Serialization;

namespace CPS.ComplexCases.DDEI.Models.Response;

public class DdeiUserDataDto
{
  [JsonPropertyName("homeUnit")]
  public required DdeiHomeUnitDto HomeUnit { get; set; }
}

public class DdeiHomeUnitDto
{
  [JsonPropertyName("unitId")]
  public int UnitId { get; set; }
}