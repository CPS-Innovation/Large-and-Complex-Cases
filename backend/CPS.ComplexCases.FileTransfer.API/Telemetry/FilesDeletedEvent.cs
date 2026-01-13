using CPS.ComplexCases.Common.Telemetry;

namespace CPS.ComplexCases.FileTransfer.API.Telemetry;

public class FilesDeletedEvent : BaseTelemetryEvent
{
    public Guid TransferId { get; set; }
    public required string TransferDirection { get; set; }
    public long TotalFilesDeleted { get; set; }
    public long TotalFilesFailedToDelete { get; set; }
    public DateTime DeletionStartTime { get; set; }
    public DateTime DeletionEndTime { get; set; }

    public override (IDictionary<string, string> Properties, IDictionary<string, double?> Metrics) ToTelemetryEventProps()
    {
        return (new Dictionary<string, string>
        {
            { nameof(CaseId), CaseId.ToString() },
            { nameof(TransferId), TransferId.ToString() },
            { nameof(TransferDirection), TransferDirection },
        }, new Dictionary<string, double?>
        {
            { TelemetryConstants.DurationCustomDimensionName, GetDurationInMilliseconds(DeletionStartTime, DeletionEndTime) },
            { nameof(TotalFilesDeleted), TotalFilesDeleted },
            { nameof(TotalFilesFailedToDelete), TotalFilesFailedToDelete },
        });
    }
}