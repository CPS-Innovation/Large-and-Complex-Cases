namespace CPS.ComplexCases.Common.Models.Domain;

public class FileTransferInfo
{
    public string? Id { get; set; }
    public required string SourcePath { get; set; }
    public string? RelativePath { get; set; }
    public string? FullFilePath { get; set; }

    // Null when the listing does not report a size. Only providers that return it populate this,
    // so size-based decisions must treat null as unknown rather than as zero bytes.
    public long? FileSizeBytes { get; set; }
}
