namespace CPS.ComplexCases.Egress.Models.Args;

public class CreateUploadArg
{
    public required string WorkspaceId { get; set; }
    public required string FileName { get; set; }
    public long FileSize { get; set; }
    public required string FolderPath { get; set; }
}