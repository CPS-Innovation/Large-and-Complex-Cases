namespace CPS.ComplexCases.FileTransfer.API.Models.Configuration;

/// <summary>
/// Configuration for purging completed and failed orchestration and entity state after a retention period.
/// </summary>
public class PurgeRetentionConfig
{
    public const string SectionName = "FileTransfer:PurgeRetention";

    /// <summary>
    /// Number of days to retain completed, failed, or terminated transfer history before purge.
    /// Default 30 days.
    /// </summary>
    public int RetentionDays { get; set; } = 30;

    /// <summary>
    /// Cron expression for the purge timer. Default "0 0 2 * * *" (2:00 AM UTC daily).
    /// </summary>
    public string Schedule { get; set; } = "0 0 2 * * *";
}
