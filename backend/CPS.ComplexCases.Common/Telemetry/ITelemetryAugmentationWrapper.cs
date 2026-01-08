namespace CPS.ComplexCases.Common.Telemetry;

public interface ITelemetryAugmentationWrapper
{
    void RegisterUsername(string username);
    void RegisterCorrelationId(Guid correlationId);
    void RegisterCmsUserId(long cmsUserId);
}