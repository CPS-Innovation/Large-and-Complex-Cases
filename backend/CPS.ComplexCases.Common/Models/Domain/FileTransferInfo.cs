namespace CPS.ComplexCases.Common.Models.Domain;

public class FileTransferInfo
{
    public string? Id { get; set; }
    public required string SourcePath { get; set; }
    public string? RelativePath { get; set; }
    public string? FullFilePath { get; set; }
}