namespace CPS.ComplexCases.FileTransfer.API.Models.Configuration;

public class SizeConfig
{
    public int ChunkSizeBytes { get; set; } = 5242880; // Default to 5 MB
}