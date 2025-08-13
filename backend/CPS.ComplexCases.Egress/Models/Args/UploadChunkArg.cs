namespace CPS.ComplexCases.Egress.Models.Args;

public class UploadChunkArg
{
    public required string WorkspaceId { get; set; }
    public required string UploadId { get; set; }
    public required byte[] ChunkData { get; set; }
    public long? Start { get; set; }
    public long? End { get; set; }
    public long? TotalSize { get; set; }
}