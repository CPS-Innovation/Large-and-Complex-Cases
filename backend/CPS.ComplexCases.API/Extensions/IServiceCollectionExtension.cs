using CPS.ComplexCases.API.Clients.FileTransfer;
using CPS.ComplexCases.API.Domain.Configuration;
using CPS.ComplexCases.Common.Extensions;
using CPS.ComplexCases.Common.Resilience;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CPS.ComplexCases.API.Extensions;

public static class IServiceCollectionExtension
{
    public static void AddFileTransferClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FileTransferApiOptions>(configuration.GetSection("FileTransferApiOptions"));

        services.AddHttpClient<IFileTransferClient, FileTransferClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<FileTransferApiOptions>>().Value;
            if (string.IsNullOrEmpty(options.BaseUrl))
            {
                throw new ArgumentNullException(nameof(options.BaseUrl), "FileTransferApiBaseUrl configuration is missing or empty.");
            }
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.RequestTimeoutSeconds);
        })
        .AddResilienceHandler("file-transfer-retry", (pipeline, context) =>
        {
            var options = context.ServiceProvider.GetRequiredService<IOptions<FileTransferApiOptions>>().Value;
            var logger = context.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("CPS.ComplexCases.API.FileTransfer");

            // Shared helper keeps POST/PUT out of status-code retries. Circuit breaker is off —
            // FileTransfer only needs the common retry policy, not fail-fast shedding.
            pipeline.AddStandardHttpResilience(new HttpResilienceOptions
            {
                ServiceName = "FileTransfer",
                RetryAttempts = options.RetryAttempts > 0 ? options.RetryAttempts : 2,
                FirstRetryDelay = TimeSpan.FromSeconds(
                    options.FirstRetryDelaySeconds > 0 ? options.FirstRetryDelaySeconds : 1),
                CircuitBreakerFailureThreshold = 0.5,
                CircuitBreakerSamplingDuration = TimeSpan.FromSeconds(30),
                CircuitBreakerMinimumThroughput = 10,
                CircuitBreakerDurationOfBreak = TimeSpan.FromSeconds(30),
                EnableCircuitBreaker = false,
                ConcurrencyLimit = 0,
            }, logger);
        });

        services.AddTransient<IRequestFactory, RequestFactory>();
    }
}
