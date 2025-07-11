namespace CPS.ComplexCases.Common.Models.Domain;

public class DeleteFilesResult
{
    public List<string>? DeletedFiles { get; set; } = [];
    public List<FailedFileDeletion>? FailedFiles { get; set; } = [];
    public bool IsSuccessful => FailedFiles?.Count == 0;
}

public class FailedFileDeletion
{
    public required string FileId { get; set; }
    public required string Filename { get; set; }
    public required string Reason { get; set; }
}