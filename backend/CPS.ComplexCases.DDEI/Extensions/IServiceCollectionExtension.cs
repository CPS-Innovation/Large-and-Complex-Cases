
using System.Net;
using System.Net.Http.Headers;
using CPS.ComplexCases.DDEI.Client;
using CPS.ComplexCases.DDEI.Factories;
using CPS.ComplexCases.DDEI.Mappers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;

namespace CPS.ComplexCases.DDEI.Extensions;

public static class IServiceCollectionExtension
{
  private const int RetryAttempts = 1;
  private const int FirstRetryDelaySeconds = 1;

  public static void AddDdeiClient(this IServiceCollection services, IConfiguration configuration)
  {
    services.Configure<DDEIOptions>(configuration.GetSection(nameof(DDEIOptions)));
    services.AddTransient<IDdeiArgFactory, DdeiArgFactory>();
    services.AddHttpClient<IDdeiClient, DdeiClient>(AddDdeiClient)
      .SetHandlerLifetime(TimeSpan.FromMinutes(5))
      .AddPolicyHandler(GetRetryPolicy());
    services.AddTransient<IDdeiRequestFactory, DdeiRequestFactory>();
    services.AddTransient<ICaseDetailsMapper, CaseDetailsMapper>();
    services.AddTransient<IAreasMapper, AreasMapper>();
    services.AddTransient<IMockSwitch, MockSwitch>();
  }

  internal static void AddDdeiClient(IServiceProvider configuration, HttpClient client)
  {
    var opts = configuration.GetService<IOptions<DDEIOptions>>()?.Value ?? throw new ArgumentNullException(nameof(DDEIOptions));

    // client.BaseAddress = new Uri(opts.BaseUrl);
    client.DefaultRequestHeaders.Add(DDEIOptions.FunctionKey, opts.AccessKey);
    client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };

    if (opts.BaseUrl.Contains(DDEIOptions.DevtunnelUrlFragment) && !string.IsNullOrWhiteSpace(DDEIOptions.DevtunnelTokenKey))
    {
      client.DefaultRequestHeaders.Add(DDEIOptions.DevtunnelTokenKey, opts.DevtunnelToken);
    }
  }

  private static AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy()
  {
    // https://learn.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/implement-http-call-retries-exponential-backoff-polly#add-a-jitter-strategy-to-the-retry-policy
    var delay = Backoff.DecorrelatedJitterBackoffV2(
        medianFirstRetryDelay: TimeSpan.FromSeconds(FirstRetryDelaySeconds),
        retryCount: RetryAttempts);

    static bool responseStatusCodePredicate(HttpResponseMessage response) =>
        response.StatusCode >= HttpStatusCode.InternalServerError
        || response.StatusCode == HttpStatusCode.NotFound;

    static bool methodPredicate(HttpResponseMessage response) =>
        response.RequestMessage?.Method != HttpMethod.Post
        && response.RequestMessage?.Method != HttpMethod.Put;

    return Policy
        .HandleResult<HttpResponseMessage>(r => responseStatusCodePredicate(r) && methodPredicate(r))
        .WaitAndRetryAsync(delay);
  }
}