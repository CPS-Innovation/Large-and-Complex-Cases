
namespace CPS.ComplexCases.ActivityLog.Models;


public class FileRecordCsvDto
{
    public string? Path { get; set; }
    public string? FileName { get; set; }
    public FileRecordStatus Status { get; set; }
}

public enum FileRecordStatus
{
    Success,
    Fail
}