namespace CPS.ComplexCases.API.Domain.Response;

public class MoveNetAppBatchResponse
{
    /// <summary>
    /// Overall outcome of the batch.
    /// Completed: every item moved successfully.
    /// PartiallyCompleted: at least one item moved and at least one was NotFound, AlreadyInPlace, Failed, or Conflict.
    /// Failed: no items moved; at least one Failed or Conflict.
    /// NoOp: no items needed moving (all were NotFound and/or AlreadyInPlace).
    /// Succeeded + NotFound + AlreadyInPlace + Failed equals TotalRequested.
    /// </summary>
    public string Status { get; set; } = string.Empty;
    public int TotalRequested { get; set; }
    public int Succeeded { get; set; }
    public int NotFound { get; set; }
    public int AlreadyInPlace { get; set; }
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
