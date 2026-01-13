using CPS.ComplexCases.Common.Telemetry;

namespace CPS.ComplexCases.Common.TelemetryEvents;

public class ActivityLogTelemetryEvent : BaseTelemetryEvent
{
    public string? ActionType { get; set; }
    public string? ResourceType { get; set; }
    public string? ResourceId { get; set; }
    public string? UserName { get; set; }
    public string? TransferId { get; set; }
    public string? TransferType { get; set; }
    public string? TransferDirection { get; set; }
    public string? SourcePath { get; set; }
    public string? DestinationPath { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public double? TotalFiles { get; set; }
    public double? TransferredFiles { get; set; }
    public double? ErrorFiles { get; set; }
    public double? TotalBytes { get; set; }

    public override (IDictionary<string, string> Properties, IDictionary<string, double?> Metrics) ToTelemetryEventProps()
    {
        var properties = new Dictionary<string, string>();
        var metrics = new Dictionary<string, double?>();

        properties["CaseId"] = CaseId.ToString();

        if (!string.IsNullOrEmpty(ActionType))
            properties["ActionType"] = ActionType;

        if (!string.IsNullOrEmpty(ResourceType))
            properties["ResourceType"] = ResourceType;

        if (!string.IsNullOrEmpty(ResourceId))
            properties["ResourceId"] = ResourceId;

        if (!string.IsNullOrEmpty(UserName))
            properties["UserName"] = UserName;

        if (!string.IsNullOrEmpty(TransferId))
            properties["TransferId"] = TransferId;

        if (!string.IsNullOrEmpty(TransferType))
            properties["TransferType"] = TransferType;

        if (!string.IsNullOrEmpty(TransferDirection))
            properties["TransferDirection"] = TransferDirection;

        if (!string.IsNullOrEmpty(SourcePath))
            properties["SourcePath"] = SourcePath;

        if (!string.IsNullOrEmpty(DestinationPath))
            properties["DestinationPath"] = DestinationPath;

        if (StartTime.HasValue)
            properties["StartTime"] = StartTime.Value.ToString("o"); // ISO 8601 format

        if (EndTime.HasValue)
            properties["EndTime"] = EndTime.Value.ToString("o");

        if (StartTime.HasValue && EndTime.HasValue && EndTime > StartTime)
        {
            var duration = (EndTime.Value - StartTime.Value).TotalSeconds;
            metrics["DurationSeconds"] = duration;
        }

        metrics["TotalFiles"] = TotalFiles;
        metrics["TransferredFiles"] = TransferredFiles;
        metrics["ErrorFiles"] = ErrorFiles;
        metrics["TotalBytes"] = TotalBytes;

        if (StartTime.HasValue && EndTime.HasValue)
        {
            var duration = (EndTime.Value - StartTime.Value).TotalSeconds;
            metrics["DurationSeconds"] = duration;

            // Calculate transfer speed (bytes per second)
            if (TotalBytes.HasValue && duration > 0)
            {
                metrics["TransferSpeedBytesPerSecond"] = TotalBytes.Value / duration;
            }
        }

        return (properties, metrics);
    }
}