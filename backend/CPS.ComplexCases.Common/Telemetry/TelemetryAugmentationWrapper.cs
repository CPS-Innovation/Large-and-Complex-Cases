using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.Common.Telemetry;

public class TelemetryAugmentationWrapper(ILogger<TelemetryAugmentationWrapper> logger) : ITelemetryAugmentationWrapper
{
    private readonly ILogger<TelemetryAugmentationWrapper> _logger = logger;

    public void RegisterUsername(string username)
    {
        RegisterCustomDimension(TelemetryConstants.UserCustomDimensionName, username);
    }

    public void RegisterCorrelationId(Guid correlationId)
    {
        RegisterCustomDimension(TelemetryConstants.CorrelationIdCustomDimensionName, correlationId.ToString());
    }

    public void RegisterCmsUserId(long cmsUserId)
    {
        RegisterCustomDimension(TelemetryConstants.CmsUserIdCustomDimensionName, cmsUserId.ToString());
    }

    public void RegisterCaseId(int caseId)
    {
        RegisterCustomDimension(TelemetryConstants.CaseIdCustomDimensionName, caseId.ToString());
    }

    private void RegisterCustomDimension(string key, string value)
    {
        var activity = Activity.Current;
        if (activity == null)
        {
            _logger.LogWarning("No current activity found when attempting to register custom dimension {Key}.", key);
            return;
        }

        try
        {
            activity?.AddBaggage(key, value);
        }
        catch (Exception exception)
        {
            throw new CriticalTelemetryException($"Failed to register custom dimension {key}.", exception);
        }
    }
}