namespace CPS.ComplexCases.FileTransfer.API.Models.Configuration;

/// <summary>
/// Configuration for the timer-triggered cleanup of stale case_active_manage_materials rows.
/// </summary>
public class ManageMaterialsCleanupConfig
{
    public const string SectionName = "FileTransfer:ManageMaterialsCleanup";

    /// <summary>
    /// Maximum age in hours before a row is deleted regardless of orchestration status.
    /// Default 24 hours.
    /// </summary>
    public int MaxAgeHours { get; set; } = 24;
}
