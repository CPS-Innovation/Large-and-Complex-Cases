using System.ComponentModel.DataAnnotations;

namespace CPS.ComplexCases.API.Durable.Payloads;

public class TransferMaterialOrchestrationPayload(string workspaceId, string documentId)
{
  [Required]
  public string WorkspaceId { get; set; } = workspaceId;
  [Required]
  public string DocumentId { get; set; } = documentId;
}