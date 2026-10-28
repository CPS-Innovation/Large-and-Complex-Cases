namespace CPS.ComplexCases.API.Domain.Configuration;

public class EgressTemplateOptions
{
    public const string SectionName = "EgressTemplateMappings";

    public string DefaultTemplateId { get; set; } = string.Empty;
    public string DefaultTemplateName { get; set; } = "CPS Areas";
    public string CentralCaseworkDivisionsTemplateId { get; set; } = string.Empty;
    public string CentralCaseworkDivisionsTemplateName { get; set; } = "Central Casework Divisions";

    private string _centralCaseworkDivisionsAreaNames = string.Empty;
    private IReadOnlySet<string> _parsedAreaNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    // Comma-separated so the list can be held in a single app setting.
    public string CentralCaseworkDivisionsAreaNames
    {
        get => _centralCaseworkDivisionsAreaNames;
        set
        {
            _centralCaseworkDivisionsAreaNames = value ?? string.Empty;
            _parsedAreaNames = _centralCaseworkDivisionsAreaNames
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
    }

    public IReadOnlySet<string> GetCentralCaseworkDivisionsAreaNames() => _parsedAreaNames;
}
