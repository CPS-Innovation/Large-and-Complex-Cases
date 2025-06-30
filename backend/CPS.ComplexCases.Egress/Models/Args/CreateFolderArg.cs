namespace CPS.ComplexCases.Egress.Models.Args;

public class CreateFolderArg
{
    public required string WorkspaceId { get; set; }
    public required string FolderName { get; set; }
    public string? Path { get; set; }
}