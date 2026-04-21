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
  [JsonPropertyName("activeTransferId")]
  public Guid? ActiveTransferId { get; set; }
  [JsonPropertyName("activeManageMaterialsOperations")]
  public List<ActiveManageMaterialsOperationResponse> ActiveManageMaterialsOperations { get; set; } = [];
}

public class ActiveManageMaterialsOperationResponse
{
  [JsonPropertyName("id")]
  public Guid Id { get; set; }
  [JsonPropertyName("operationType")]
  public required string OperationType { get; set; }
  [JsonPropertyName("sourcePaths")]
  public required string SourcePaths { get; set; }
  [JsonPropertyName("destinationPaths")]
  public string? DestinationPaths { get; set; }
  [JsonPropertyName("userName")]
  public string? UserName { get; set; }
  [JsonPropertyName("createdAt")]
  public DateTime CreatedAt { get; set; }
}