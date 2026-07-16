namespace CPS.ComplexCases.Common.Helpers;

public static class NetAppBatchOutcome
{
    /// <summary>
    /// Resolves overall batch status from per-item counts.
    /// Completed only when every item succeeded; any NotFound/skipped alongside success is PartiallyCompleted.
    /// </summary>
    public static string ResolveStatus(int succeeded, int failed, int notFoundOrSkipped) =>
        (succeeded > 0, failed > 0, notFoundOrSkipped > 0) switch
        {
            (false, false, _) => "NoOp",
            (true, false, false) => "Completed",
            (false, true, _) => "Failed",
            _ => "PartiallyCompleted",
        };
}
