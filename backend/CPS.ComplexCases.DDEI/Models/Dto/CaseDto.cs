using System.Text.Json.Serialization;

namespace CPS.ComplexCases.DDEI.Models.Dto;

public class CaseDto
{
  [JsonPropertyName("caseId")]
  public int CaseId { get; set; }
  [JsonPropertyName("urn")]
  public required string Urn { get; set; }
  [JsonPropertyName("operationName")]
  public string? OperationName { get; set; }
  [JsonPropertyName("leadDefendantName")]
  public string? LeadDefendantName { get; set; }
  [JsonPropertyName("registrationDate")]
  public string? RegistrationDate { get; set; }
}