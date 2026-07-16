namespace CPS.ComplexCases.API.Domain.Response;

public class DeleteNetAppBatchResponse
{
    /// <summary>
    /// Overall outcome of the batch.
    /// Completed: every item deleted successfully.
    /// PartiallyCompleted: at least one item deleted and at least one was NotFound or Failed.
    /// Failed: no items deleted; at least one Failed.
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
