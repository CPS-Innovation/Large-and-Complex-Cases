
using System.Net;
using System.Net.Http.Headers;
using CPS.ComplexCases.Common.Extensions;
using CPS.ComplexCases.Common.Resilience;
using CPS.ComplexCases.DDEI.Client;
using CPS.ComplexCases.DDEI.Factories;
using CPS.ComplexCases.DDEI.Mappers;
using CPS.ComplexCases.DDEI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CPS.ComplexCases.DDEI.Extensions;

public static class IServiceCollectionExtension
{
  private const int RetryAttempts = 1;
  private const int FirstRetryDelaySeconds = 1;

  // MDS (via DDEI) is a standard request/response service, so a 30s sampling window
  // with a moderate throughput requirement is enough to spot a failing service quickly.
  private const double CircuitBreakerFailureThreshold = 0.5;
  private const int CircuitBreakerSamplingDurationSeconds = 30;
  private const int CircuitBreakerMinimumThroughput = 10;
  private const int CircuitBreakerDurationOfBreakSeconds = 30;

  public static void AddDdeiClient(this IServiceCollection services, IConfiguration configuration)
  {
    services.Configure<DDEIOptions>(configuration.GetSection(nameof(DDEIOptions)));
    services.AddTransient<IDdeiArgFactory, DdeiArgFactory>();

    // A 404 is retried (MDS occasionally returns one transiently) but is not a health signal, so it is
    // deliberately excluded from the breaker. Connection failures are also retried for this service.
    var configureResilience = ResiliencePipelineExtensions.ConfigureStandardResilience(
        "CPS.ComplexCases.DDEI.CircuitBreaker",
        new HttpResilienceOptions
        {
          ServiceName = "MDS (DDEI)",
          RetryAttempts = RetryAttempts,
          FirstRetryDelay = TimeSpan.FromSeconds(FirstRetryDelaySeconds),
          CircuitBreakerFailureThreshold = CircuitBreakerFailureThreshold,
          CircuitBreakerSamplingDuration = TimeSpan.FromSeconds(CircuitBreakerSamplingDurationSeconds),
          CircuitBreakerMinimumThroughput = CircuitBreakerMinimumThroughput,
          CircuitBreakerDurationOfBreak = TimeSpan.FromSeconds(CircuitBreakerDurationOfBreakSeconds),
          ConcurrencyLimit = 0,
          RetryOnConnectionFailure = true,
          AdditionalRetryableStatusCodes = [HttpStatusCode.NotFound],
        });

    services.AddHttpClient<IDdeiClient, DdeiClient>(AddDdeiClient)
      .SetHandlerLifetime(TimeSpan.FromMinutes(5))
      .AddResilienceHandler("ddei-resilience", configureResilience);
    services.AddTransient<IDdeiRequestFactory, DdeiRequestFactory>();
    services.AddTransient<ICaseDetailsMapper, CaseDetailsMapper>();
    services.AddTransient<IAreasMapper, AreasMapper>();
    services.AddTransient<IMockSwitch, MockSwitch>();
    services.AddSingleton<ICaseNamingService, CaseNamingService>();
  }

  internal static void AddDdeiClient(IServiceProvider configuration, HttpClient client)
  {
    var opts = configuration.GetService<IOptions<DDEIOptions>>()?.Value ?? throw new ArgumentNullException(nameof(DDEIOptions));
    client.DefaultRequestHeaders.Add(DDEIOptions.FunctionKey, opts.AccessKey);
    client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
    client.Timeout = TimeSpan.FromSeconds(opts.RequestTimeoutSeconds);

    if (opts.BaseUrl.Contains(DDEIOptions.DevtunnelUrlFragment) && !string.IsNullOrWhiteSpace(DDEIOptions.DevtunnelTokenKey))
    {
      client.DefaultRequestHeaders.Add(DDEIOptions.DevtunnelTokenKey, opts.DevtunnelToken);
    }
  }
}
