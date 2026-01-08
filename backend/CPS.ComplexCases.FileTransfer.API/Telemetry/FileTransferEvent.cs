using CPS.ComplexCases.Common.Telemetry;

namespace CPS.ComplexCases.FileTransfer.API.Telemetry;

public class FileTransferEvent : BaseTelemetryEvent
{
    public long FileSizeInBytes { get; set; }
    public bool IsSuccessful { get; set; }
    public bool IsMultipart { get; set; }
    public int? TotalPartsCount { get; set; }
    public DateTime TransferStartTime { get; set; }
    public DateTime TransferEndTime { get; set; }

    public override (IDictionary<string, string> Properties, IDictionary<string, double?> Metrics) ToTelemetryEventProps()
    {
        return (new Dictionary<string, string>
        {
            { nameof(CaseId), CaseId.ToString() },
            { nameof(IsSuccessful), IsSuccessful.ToString() },
            { nameof(IsMultipart), IsMultipart.ToString() }
        }, new Dictionary<string, double?>
        {
            { nameof(FileSizeInBytes), FileSizeInBytes },
            { nameof(TelemetryConstants.DurationCustomDimensionName), GetDurationInMilliseconds(TransferStartTime, TransferEndTime) },
            { nameof(TotalPartsCount), IsMultipart ? TotalPartsCount : 1 }
        });
    }
}