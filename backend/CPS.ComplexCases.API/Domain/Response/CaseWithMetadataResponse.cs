using System.Text.Json.Serialization;

namespace CPS.ComplexCases.API.Domain.Response;

public class CaseWithMetadataResponse
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
  [JsonPropertyName("egressWorkspaceId")]
  public string? EgressWorkspaceId { get; set; }
  [JsonPropertyName("netappFolderPath")]
  public string? NetappFolderPath { get; set; }
}