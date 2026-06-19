namespace CPS.ComplexCases.API.Domain.Configuration;

public class FileTransferApiOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public int RetryAttempts { get; set; } = 2;
    public int FirstRetryDelaySeconds { get; set; } = 1;

    // Trigger/status calls return quickly; the heavy work runs asynchronously in durable
    // orchestrations.
    public int RequestTimeoutSeconds { get; set; } = 100;
}