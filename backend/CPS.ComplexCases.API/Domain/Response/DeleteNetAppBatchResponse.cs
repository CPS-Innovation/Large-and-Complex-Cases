namespace CPS.ComplexCases.API.Domain.Response;

public class DeleteNetAppBatchResponse
{
    public int TotalRequested { get; set; }
    public int Succeeded { get; set; }
    public int Failed { get; set; }
    public List<DeleteNetAppBatchItemResult> Results { get; set; } = [];
}

public class DeleteNetAppBatchItemResult
{
    public string SourcePath { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Error { get; set; }
    public int? KeysDeleted { get; set; }
}
