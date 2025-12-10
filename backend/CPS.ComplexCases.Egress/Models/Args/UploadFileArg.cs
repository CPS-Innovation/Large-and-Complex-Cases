namespace CPS.ComplexCases.Egress.Models.Args;

public class UploadFileArg
{
    public required string WorkspaceId { get; set; }
    public required string FolderPath { get; set; }
    public required string FileName { get; set; }
    public required Stream FileStream { get; set; }
}
