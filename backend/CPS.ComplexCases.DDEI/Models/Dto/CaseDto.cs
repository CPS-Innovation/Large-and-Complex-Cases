using System.Text.Json.Serialization;

namespace CPS.ComplexCases.DDEI.Models.Dto;

public class CaseDto
{
  [JsonPropertyName("caseId")]
  public int CaseId { get; set; }
  [JsonPropertyName("urn")]
  public required string Urn { get; set; }
  // [JsonPropertyName("operationName")]
  // public required string OperationName { get; set; }
  [JsonPropertyName("leadDefendantName")]
  public required string LeadDefendantName { get; set; }
}