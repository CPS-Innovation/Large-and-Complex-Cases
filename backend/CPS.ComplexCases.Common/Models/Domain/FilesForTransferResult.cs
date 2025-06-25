namespace CPS.ComplexCases.Common.Models.Domain;

public class FilesForTransferResult
{
    public required int CaseId { get; set; }
    public string? WorkspaceId { get; set; }
    public required string TransferDirection { get; set; }
    public required IEnumerable<FileTransferInfo> Files { get; set; }
    public IEnumerable<string>? ValidationErrors { get; set; }
    public bool IsInvalid { get; set; } = false;
}