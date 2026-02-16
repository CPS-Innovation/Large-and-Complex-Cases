using CPS.ComplexCases.Common.Telemetry;

namespace CPS.ComplexCases.FileTransfer.API.Telemetry;

public class DuplicateFileConflictEvent : BaseTelemetryEvent
{
    public string SourceFilePath { get; set; } = string.Empty;
    public string DestinationFilePath { get; set; } = string.Empty;
    public string ConflictingFileName { get; set; } = string.Empty;
    public string TransferDirection { get; set; } = string.Empty;
    public string TransferId { get; set; } = string.Empty;

    public override (IDictionary<string, string> Properties, IDictionary<string, double?> Metrics) ToTelemetryEventProps()
    {
        return (new Dictionary<string, string>
        {
            { nameof(CaseId), CaseId.ToString() },
            { nameof(SourceFilePath), SourceFilePath },
            { nameof(DestinationFilePath), DestinationFilePath },
            { nameof(ConflictingFileName), ConflictingFileName },
            { nameof(TransferDirection), TransferDirection },
            { nameof(TransferId), TransferId }
        }, new Dictionary<string, double?>
        {
        });
    }
}