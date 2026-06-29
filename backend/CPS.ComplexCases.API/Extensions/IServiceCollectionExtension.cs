using System.Net;
using CPS.ComplexCases.API.Clients.FileTransfer;
using CPS.ComplexCases.API.Domain.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;

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
            var retryAttempts = options.RetryAttempts > 0 ? options.RetryAttempts : 2;
            var firstRetryDelaySeconds = options.FirstRetryDelaySeconds > 0 ? options.FirstRetryDelaySeconds : 1;

            pipeline.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = retryAttempts,
                Delay = TimeSpan.FromSeconds(firstRetryDelaySeconds),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = static args =>
                    ValueTask.FromResult(args.Outcome.Result?.StatusCode >= HttpStatusCode.InternalServerError)
            });
        });

        services.AddTransient<IRequestFactory, RequestFactory>();
    }
}