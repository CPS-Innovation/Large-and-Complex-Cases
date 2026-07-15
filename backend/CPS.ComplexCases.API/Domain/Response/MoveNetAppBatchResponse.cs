namespace CPS.ComplexCases.API.Domain.Response;

public class MoveNetAppBatchResponse
{
    /// <summary>
    /// Overall outcome of the batch.
    /// Completed: all items moved with no failures.
    /// PartiallyCompleted: at least one item moved and at least one failed.
    /// Failed: no items moved; all that were attempted resulted in failure/conflict.
    /// NoOp: no items needed moving (all were NotFound).
    /// </summary>
    public string Status { get; set; } = string.Empty;
    public int TotalRequested { get; set; }
    public int Succeeded { get; set; }
    public int NotFound { get; set; }
    public int Failed { get; set; }
    public List<MoveNetAppBatchItemResult> Results { get; set; } = [];
}

public class MoveNetAppBatchItemResult
{
    public string Type { get; set; } = string.Empty;
    public string SourcePath { get; set; } = string.Empty;
    public string DestinationPath { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Error { get; set; }
}
