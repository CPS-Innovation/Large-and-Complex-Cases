namespace CPS.ComplexCases.Egress.Models.Args;

public class DeleteFilesArg
{
    public required string WorkspaceId { get; set; }
    public required List<string> FileIds { get; set; } = [];
}