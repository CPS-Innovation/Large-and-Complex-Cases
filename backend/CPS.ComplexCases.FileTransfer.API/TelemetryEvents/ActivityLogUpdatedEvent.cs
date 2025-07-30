namespace CPS.ComplexCases.FileTransfer.API.TelemetryEvents;

public class ActivityLogUpdatedEvent : BaseTransferEvent
{
    public ActivityLogUpdatedEvent(Guid correlationId, long caseId)
    {
        CorrelationId = correlationId;
        CaseId = caseId;
    }

    public int TransferCount { get; set; }

    public override (IDictionary<string, string>, IDictionary<string, double?>) ToTelemetryEventProps()
    {
        var baseProps = base.ToTelemetryEventProps();

        baseProps.Item2.Add(nameof(TransferCount), TransferCount);

        return baseProps;
    }
}