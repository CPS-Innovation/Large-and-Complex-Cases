namespace CPS.ComplexCases.API.Domain.Configuration;

public class EgressTemplateOptions
{
    public const string SectionName = "EgressTemplateMappings";

    public string DefaultTemplateId { get; set; } = string.Empty;
    public string DefaultTemplateName { get; set; } = "CPS Areas";
    public string CentralCaseworkDivisionsTemplateId { get; set; } = string.Empty;
    public string CentralCaseworkDivisionsTemplateName { get; set; } = "Central Casework Divisions";

    // Comma-separated so the list can be held in a single app setting.
    public string CentralCaseworkDivisionsAreaNames { get; set; } = string.Empty;

    public IReadOnlySet<string> GetCentralCaseworkDivisionsAreaNames() =>
        CentralCaseworkDivisionsAreaNames
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
}
