namespace CPS.ComplexCases.Common.Models.Domain;

public class UploadSession
{
    public required string UploadId { get; set; }
    public string? WorkspaceId { get; set; }
    public string? Md5Hash { get; set; }
}