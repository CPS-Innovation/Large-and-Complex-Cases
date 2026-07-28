namespace CPS.ComplexCases.API.Domain.Response;

public class MaterialRenameBatchResponse
{
    /// <summary>
    /// Overall outcome of the batch.
    /// Completed: every item renamed successfully.
    /// PartiallyCompleted: at least one item renamed and at least one was NotFound or Failed.
    /// Failed: no items renamed; at least one Failed.
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
}
