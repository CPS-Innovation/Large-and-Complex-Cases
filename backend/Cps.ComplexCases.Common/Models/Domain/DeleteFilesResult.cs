namespace CPS.ComplexCases.Common.Models.Domain;

public class DeleteFilesResult
{
    public List<string> DeletedFiles { get; set; } = [];
    public List<string> FailedFiles { get; set; } = [];
    public bool IsSuccessful => FailedFiles.Count == 0;
}