using CPS.ComplexCases.Common.Telemetry;

namespace CPS.ComplexCases.Common.Handlers;

public class InitializationHandler(ITelemetryAugmentationWrapper telemetryAugmentationWrapper) : IInitializationHandler
{
    private readonly ITelemetryAugmentationWrapper _telemetryAugmentationWrapper = telemetryAugmentationWrapper;

    public void Initialize(string username, Guid? correlationId)
    {
        if (!string.IsNullOrEmpty(username))
        {
            _telemetryAugmentationWrapper.RegisterUsername(username);
        }
        if (correlationId != null)
        {
            _telemetryAugmentationWrapper.RegisterCorrelationId(correlationId.Value);
        }
    }
}