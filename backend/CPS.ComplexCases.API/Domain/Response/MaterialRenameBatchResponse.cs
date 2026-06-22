namespace CPS.ComplexCases.API.Domain.Response;

public class MaterialRenameBatchResponse
{
    /// <summary>
    /// Overall outcome of the batch.
    /// Completed: all items processed with no failures (some may be NotFound).
    /// PartiallyCompleted: at least one item succeeded and at least one failed.
    /// Failed: no items succeeded; all that were attempted resulted in failure.
    /// NoOp: no items needed renaming (all were NotFound).
    /// </summary>
    public string Status { get; set; } = string.Empty;
    public int TotalRequested { get; set; }
    public int Succeeded { get; set; }
    public int NotFound { get; set; }
    public int Failed { get; set; }
    public List<MaterialRenameBatchItemResult> Results { get; set; } = [];
}

public class MaterialRenameBatchItemResult
{
    public string PreviousPath { get; set; } = string.Empty;
    public string NewPath { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Error { get; set; }
    public int? KeysRenamed { get; set; }
}
