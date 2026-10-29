namespace CPS.ComplexCases.API.Services;

public record EgressTemplateResolution(string TemplateId, string TemplateName, bool IsDefaultFallback);

public interface IEgressTemplateResolver
{
    /// <summary>
    /// Resolves the Egress workspace template for a case area. Areas configured as Central Casework
    /// Divisions get that template; any other or unassigned area gets the default "CPS Areas" template.
    /// </summary>
    EgressTemplateResolution ResolveTemplate(string? areaName);
}
