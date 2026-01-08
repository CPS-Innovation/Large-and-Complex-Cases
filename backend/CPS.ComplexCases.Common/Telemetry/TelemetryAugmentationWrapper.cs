using System.Diagnostics;

namespace CPS.ComplexCases.Common.Telemetry;

public class TelemetryAugmentationWrapper : ITelemetryAugmentationWrapper
{
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

    private static void RegisterCustomDimension(string key, string value)
    {
        var activity = Activity.Current
            ?? throw new CriticalTelemetryException("System.Diagnostics.Activity.Current was expected but was null.", new InvalidOperationException());

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