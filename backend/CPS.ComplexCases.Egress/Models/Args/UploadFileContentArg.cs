namespace CPS.ComplexCases.Egress.Models.Args;

public class UploadFileContentArg
{
    public required string WorkspaceId { get; set; }
    public required string UploadId { get; set; }
    public required byte[] FileContent { get; set; }
}
