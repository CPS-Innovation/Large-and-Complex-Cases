using System.Net;
using CPS.ComplexCases.API.Clients.FileTransfer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Contrib.WaitAndRetry;

namespace CPS.ComplexCases.API.Extensions;

public static class IServiceCollectionExtension
{
    private const int RetryAttempts = 2;
    private const int FirstRetryDelaySeconds = 1;
    public static void AddFileTransferClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<IFileTransferClient, FileTransferClient>(client =>
        {
            var fileTransferApiUrl = configuration["FileTransferApiOptions:BaseUrl"];
            if (string.IsNullOrEmpty(fileTransferApiUrl))
            {
                throw new ArgumentNullException(nameof(fileTransferApiUrl), "FileTransferApiBaseUrl configuration is missing or empty.");
            }
            client.BaseAddress = new Uri(fileTransferApiUrl);
        }).AddPolicyHandler(RetryPolicy);

        services.AddTransient<IRequestFactory, RequestFactory>();
    }

    private static IAsyncPolicy<HttpResponseMessage> RetryPolicy =>
    // https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-http-call-retries-exponential-backoff-polly#add-a-jitter-strategy-to-the-retry-policy
    Policy
        .HandleResult<HttpResponseMessage>((result) => ShouldRetry(result))
        .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(
            medianFirstRetryDelay: TimeSpan.FromSeconds(FirstRetryDelaySeconds),
            retryCount: RetryAttempts));

    private static bool ShouldRetry(HttpResponseMessage response)
    {

        return response.StatusCode >= HttpStatusCode.InternalServerError;
    }
}