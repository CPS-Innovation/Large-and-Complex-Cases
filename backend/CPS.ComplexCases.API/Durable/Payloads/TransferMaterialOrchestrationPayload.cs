using System.ComponentModel.DataAnnotations;

namespace CPS.ComplexCases.API.Durable.Payloads;

public class TransferMaterialOrchestrationPayload(string workspaceId, string documentId, string destinationPath)
{
  [Required]
  public string WorkspaceId { get; set; } = workspaceId;
  [Required]
  public string DocumentId { get; set; } = documentId;
  [Required]
  public string DestinationPath { get; set; } = destinationPath;
}