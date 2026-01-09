using CPS.ComplexCases.Common.Telemetry;

namespace CPS.ComplexCases.FileTransfer.API.Telemetry;

public class TransferOrchestrationEvent : BaseTelemetryEvent
{
    public required string TransferDirection { get; set; }
    public required string BucketName { get; set; }
    public long TotalFiles { get; set; }
    public long TotalFilesTransferred { get; set; }
    public long TotalBytesTransferred { get; set; }
    public long TotalFilesFailed { get; set; }
    public DateTime OrchestrationStartTime { get; set; }
    public DateTime OrchestrationEndTime { get; set; }
    public bool IsSuccessful { get; set; }

    public override (IDictionary<string, string> Properties, IDictionary<string, double?> Metrics) ToTelemetryEventProps()
    {
        return (new Dictionary<string, string>
        {
            { nameof(CaseId), CaseId.ToString() },
            { nameof(TransferDirection), TransferDirection },
            { nameof(BucketName), BucketName },
            { nameof(IsSuccessful), IsSuccessful.ToString() }
        }, new Dictionary<string, double?>
        {
            { nameof(TotalFilesTransferred), TotalFilesTransferred },
            { nameof(TotalBytesTransferred), TotalBytesTransferred },
            { nameof(TotalFilesFailed), TotalFilesFailed },
            { TelemetryConstants.DurationCustomDimensionName, GetDurationInMilliseconds(OrchestrationStartTime, OrchestrationEndTime) }
        });
    }
}