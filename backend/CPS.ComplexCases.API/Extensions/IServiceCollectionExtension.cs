using System.Net;
using CPS.ComplexCases.API.Clients.FileTransfer;
using CPS.ComplexCases.API.Domain.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Contrib.WaitAndRetry;

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
        })
        .AddPolicyHandler((serviceProvider, request) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<FileTransferApiOptions>>().Value;
            return CreateRetryPolicy(options);
        });

        services.AddTransient<IRequestFactory, RequestFactory>();
    }

    private static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy(FileTransferApiOptions options)
    {
        var retryAttempts = options.RetryAttempts > 0 ? options.RetryAttempts : 2;
        var firstRetryDelaySeconds = options.FirstRetryDelaySeconds > 0 ? options.FirstRetryDelaySeconds : 1;

        return Policy
            .HandleResult<HttpResponseMessage>(ShouldRetry)
            .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(
                medianFirstRetryDelay: TimeSpan.FromSeconds(firstRetryDelaySeconds),
                retryCount: retryAttempts));
    }

    private static bool ShouldRetry(HttpResponseMessage response)
    {
        return response.StatusCode >= HttpStatusCode.InternalServerError;
    }
}