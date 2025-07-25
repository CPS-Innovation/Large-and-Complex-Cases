namespace CPS.ComplexCases.Egress.Models.Args;

public class UploadChunkArg
{
    public required string WorkspaceId { get; set; }
    public required string UploadId { get; set; }
    public required byte[] ChunkData { get; set; }
    public string? ContentRange { get; set; }
}