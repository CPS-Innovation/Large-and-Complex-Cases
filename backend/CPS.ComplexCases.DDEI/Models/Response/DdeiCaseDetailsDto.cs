using System.Text.Json.Serialization;

namespace CPS.ComplexCases.DDEI.Models.Response;

public class DdeiCaseDetailsDto
{
  [JsonPropertyName("summary")]
  public required DdeiCaseSummaryDto Summary { get; set; }
}