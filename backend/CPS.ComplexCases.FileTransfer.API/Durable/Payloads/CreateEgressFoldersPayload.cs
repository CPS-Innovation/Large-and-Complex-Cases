namespace CPS.ComplexCases.FileTransfer.API.Durable.Payloads;

public class CreateEgressFoldersPayload
{
    public required string WorkspaceId { get; set; }
    public required List<string> FolderPaths { get; set; }
    public int CaseId { get; set; }
    public string? UserName { get; set; }
    public Guid? CorrelationId { get; set; }
}
