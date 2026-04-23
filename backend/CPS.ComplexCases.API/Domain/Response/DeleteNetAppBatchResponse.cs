namespace CPS.ComplexCases.API.Domain.Response;

public class DeleteNetAppBatchResponse
{
    /// <summary>
    /// Overall outcome of the batch.
    /// Completed: all items processed with no failures (some may be NotFound).
    /// PartiallyCompleted: at least one item succeeded and at least one failed.
    /// Failed: no items succeeded; all that were attempted resulted in failure.
    /// NoOp: no items needed deleting (all were NotFound).
    /// </summary>
    public string Status { get; set; } = string.Empty;
    public int TotalRequested { get; set; }
    public int Succeeded { get; set; }
    public int NotFound { get; set; }
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
