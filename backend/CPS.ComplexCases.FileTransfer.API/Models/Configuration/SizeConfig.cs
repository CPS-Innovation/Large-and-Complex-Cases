namespace CPS.ComplexCases.FileTransfer.API.Models.Configuration;

public class SizeConfig
{
    public int ChunkSizeBytes { get; set; } = 8 * 1024 * 1024; // default to 8 MB
    public int MinMultipartSizeBytes { get; set; } = 5 * 1024 * 1024; // Default to 5 MB
    public int BatchSize { get; set; } = 10; // Default to 10 files per batch
    public int MaxConcurrentPartUploads { get; set; } = 4;
}