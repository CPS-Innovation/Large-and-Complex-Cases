namespace CPS.ComplexCases.Common.Telemetry;

public class ProvisionNetAppFoldersEvent : BaseTelemetryEvent
{
    public required string DestinationPath { get; set; }
    public required string TemplatePath { get; set; }
    public string? BucketName { get; set; }
    public string? TransferId { get; set; }
    public bool IsSuccessful { get; set; }
    public string? FailureReason { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public override (IDictionary<string, string> Properties, IDictionary<string, double?> Metrics) ToTelemetryEventProps()
    {
        var properties = new Dictionary<string, string>
        {
            { nameof(CaseId), CaseId.ToString() },
            { nameof(DestinationPath), DestinationPath },
            { nameof(TemplatePath), TemplatePath },
            { nameof(IsSuccessful), IsSuccessful.ToString() },
        };

        if (!string.IsNullOrEmpty(BucketName))
        {
            properties[nameof(BucketName)] = BucketName;
        }

        if (!string.IsNullOrEmpty(TransferId))
        {
            properties[nameof(TransferId)] = TransferId;
        }

        if (!string.IsNullOrEmpty(FailureReason))
        {
            properties[nameof(FailureReason)] = FailureReason;
        }

        return (properties, new Dictionary<string, double?>
        {
            { TelemetryConstants.DurationCustomDimensionName, GetDurationInMilliseconds(StartTime, EndTime) },
        });
    }
}
