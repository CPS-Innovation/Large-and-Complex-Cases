namespace CPS.ComplexCases.FileTransfer.API.Models.Configuration;

public class SizeConfig
{
    public int ChunkSizeBytes { get; set; } = 5242880; // Default to 5 MB
    public int BatchSize { get; set; } = 10; // Default to 10 files per batch
}