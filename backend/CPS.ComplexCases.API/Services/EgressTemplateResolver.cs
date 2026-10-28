using CPS.ComplexCases.API.Domain.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CPS.ComplexCases.API.Services;

public class EgressTemplateResolver(
    IOptionsMonitor<EgressTemplateOptions> options,
    ILogger<EgressTemplateResolver> logger) : IEgressTemplateResolver
{
    private readonly IOptionsMonitor<EgressTemplateOptions> _options = options;
    private readonly ILogger<EgressTemplateResolver> _logger = logger;

    public EgressTemplateResolution ResolveTemplate(string? areaName)
    {
        var options = _options.CurrentValue;

        if (string.IsNullOrWhiteSpace(options.DefaultTemplateId))
        {
            throw new InvalidOperationException(
                $"{EgressTemplateOptions.SectionName}:{nameof(EgressTemplateOptions.DefaultTemplateId)} configuration value is missing or empty.");
        }

        var defaultResolution = new EgressTemplateResolution(options.DefaultTemplateId, options.DefaultTemplateName, true);

        if (string.IsNullOrWhiteSpace(areaName) ||
            !options.GetCentralCaseworkDivisionsAreaNames().Contains(areaName.Trim()))
        {
            return defaultResolution;
        }

        if (string.IsNullOrWhiteSpace(options.CentralCaseworkDivisionsTemplateId))
        {
            _logger.LogWarning(
                "Area {AreaName} is configured as a Central Casework Division but {Setting} is not set. Falling back to the default template.",
                areaName,
                nameof(EgressTemplateOptions.CentralCaseworkDivisionsTemplateId));
            return defaultResolution;
        }

        return new EgressTemplateResolution(
            options.CentralCaseworkDivisionsTemplateId,
            options.CentralCaseworkDivisionsTemplateName,
            false);
    }
}
