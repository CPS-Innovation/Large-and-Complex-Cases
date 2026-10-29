namespace CPS.ComplexCases.Common.Telemetry;

public class EgressWorkspaceCreatedEvent : BaseTelemetryEvent
{
    public required string WorkspaceId { get; set; }
    public required string TemplateId { get; set; }
    public required string TemplateName { get; set; }
    public int? AreaId { get; set; }
    public string? AreaName { get; set; }
    public bool IsDefaultFallback { get; set; }
    public bool IsExplicitTemplate { get; set; }

    public override (IDictionary<string, string> Properties, IDictionary<string, double?> Metrics) ToTelemetryEventProps()
    {
        var properties = new Dictionary<string, string>
        {
            { nameof(CaseId), CaseId.ToString() },
            { nameof(WorkspaceId), WorkspaceId },
            { nameof(TemplateId), TemplateId },
            { nameof(TemplateName), TemplateName },
            { nameof(IsDefaultFallback), IsDefaultFallback.ToString() },
            { nameof(IsExplicitTemplate), IsExplicitTemplate.ToString() },
        };

        if (AreaId.HasValue)
        {
            properties[nameof(AreaId)] = AreaId.Value.ToString();
        }

        if (!string.IsNullOrEmpty(AreaName))
        {
            properties[nameof(AreaName)] = AreaName;
        }

        return (properties, new Dictionary<string, double?>());
    }
}
