namespace CPS.ComplexCases.Common.Telemetry;

public class CriticalTelemetryException(string message, Exception exception) : Exception(message, exception)
{
}