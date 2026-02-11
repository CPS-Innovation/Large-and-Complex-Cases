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
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }

    public override (IDictionary<string, string> Properties, IDictionary<string, double?> Metrics) ToTelemetryEventProps()
    {
        var properties = new Dictionary<string, string>
        {
            { nameof(CaseId), CaseId.ToString() },
            { nameof(IsSuccessful), IsSuccessful.ToString() },
            { nameof(IsMultipart), IsMultipart.ToString() }
        };

        if (!string.IsNullOrEmpty(ErrorCode))
            properties[nameof(ErrorCode)] = ErrorCode;

        if (!string.IsNullOrEmpty(ErrorMessage))
            properties[nameof(ErrorMessage)] = ErrorMessage;

        return (properties, new Dictionary<string, double?>
        {
            { nameof(FileSizeInBytes), FileSizeInBytes },
            { TelemetryConstants.DurationCustomDimensionName, GetDurationInMilliseconds(TransferStartTime, TransferEndTime) },
            { nameof(TotalPartsCount), IsMultipart ? TotalPartsCount : 1 }
        });
    }
}