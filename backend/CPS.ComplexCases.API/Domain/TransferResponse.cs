namespace CPS.ComplexCases.API.Domain;

public class TransferResponse(string workspaceId, string documentId)
{
  public string WorkspaceId { get; set; } = workspaceId;
  public string DocumentId { get; set; } = documentId;
}