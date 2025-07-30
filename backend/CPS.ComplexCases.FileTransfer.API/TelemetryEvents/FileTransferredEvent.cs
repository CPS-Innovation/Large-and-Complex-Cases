namespace CPS.ComplexCases.FileTransfer.API.TelemetryEvents;

public class FileTransferredEvent : BaseTransferEvent
{
    public FileTransferredEvent(Guid correlationId, long caseId, string? fileName)
    {
        CorrelationId = correlationId;
        CaseId = caseId;
        FileName = fileName;
    }
}