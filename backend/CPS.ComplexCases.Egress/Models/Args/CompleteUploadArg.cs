namespace CPS.ComplexCases.Egress.Models.Args;

public class CompleteUploadArg
{
    public required string WorkspaceId { get; set; }
    public required string UploadId { get; set; }
    public string? Md5Hash { get; set; }
    public bool Done { get; set; } = true;
}