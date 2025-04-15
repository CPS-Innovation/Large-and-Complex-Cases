
using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Data.Models.Requests;

public class CreateEgressConnectionDto
{
  [JsonPropertyName("caseId")]
  public required int CaseId { get; set; }
  [JsonPropertyName("egressWorkspaceId")]
  public required string EgressWorkspaceId { get; set; }
}
