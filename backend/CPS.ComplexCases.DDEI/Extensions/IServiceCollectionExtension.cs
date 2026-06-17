
using System.Net;
using System.Net.Http.Headers;
using CPS.ComplexCases.DDEI.Client;
using CPS.ComplexCases.DDEI.Factories;
using CPS.ComplexCases.DDEI.Mappers;
using CPS.ComplexCases.DDEI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Contrib.WaitAndRetry;

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

  // Created once on first request and reused so circuit-breaker state is shared across all calls.
  private static IAsyncPolicy<HttpResponseMessage>? _resiliencePolicy;

  public static void AddDdeiClient(this IServiceCollection services, IConfiguration configuration)
  {
    services.Configure<DDEIOptions>(configuration.GetSection(nameof(DDEIOptions)));
    services.AddTransient<IDdeiArgFactory, DdeiArgFactory>();
    services.AddHttpClient<IDdeiClient, DdeiClient>(AddDdeiClient)
      .SetHandlerLifetime(TimeSpan.FromMinutes(5))
      .AddPolicyHandler((sp, _) => _resiliencePolicy ??= GetResiliencePolicy(sp.GetRequiredService<ILoggerFactory>()));
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
    client.Timeout = TimeSpan.FromMinutes(10);

    if (opts.BaseUrl.Contains(DDEIOptions.DevtunnelUrlFragment) && !string.IsNullOrWhiteSpace(DDEIOptions.DevtunnelTokenKey))
    {
      client.DefaultRequestHeaders.Add(DDEIOptions.DevtunnelTokenKey, opts.DevtunnelToken);
    }
  }

  // Retry is kept outermost so a retry attempt re-enters the (possibly open) circuit and fails fast
  // instead of bypassing the breaker.
  internal static IAsyncPolicy<HttpResponseMessage> GetResiliencePolicy(ILoggerFactory loggerFactory)
  {
    var logger = loggerFactory.CreateLogger("CPS.ComplexCases.DDEI.CircuitBreaker");
    var circuitBreaker = GetCircuitBreakerPolicy(
        logger,
        CircuitBreakerFailureThreshold,
        TimeSpan.FromSeconds(CircuitBreakerSamplingDurationSeconds),
        CircuitBreakerMinimumThroughput,
        TimeSpan.FromSeconds(CircuitBreakerDurationOfBreakSeconds));

    return Policy.WrapAsync(GetRetryPolicy(), circuitBreaker);
  }

  private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
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
        .Handle<HttpRequestException>()
        .OrResult<HttpResponseMessage>(r => responseStatusCodePredicate(r) && methodPredicate(r))
        .WaitAndRetryAsync(delay);
  }

  // Only "service is down" signals (5xx and connection failures) trip the breaker. A 404 is not a
  // health signal, so it is deliberately excluded here even though retry handles it.
  internal static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(
      ILogger logger,
      double failureThreshold,
      TimeSpan samplingDuration,
      int minimumThroughput,
      TimeSpan durationOfBreak)
  {
    return Policy<HttpResponseMessage>
        .Handle<HttpRequestException>()
        .OrResult(r => r.StatusCode >= HttpStatusCode.InternalServerError)
        .AdvancedCircuitBreakerAsync(
            failureThreshold,
            samplingDuration,
            minimumThroughput,
            durationOfBreak,
            onBreak: (outcome, breakDelay) => logger.LogError(
                outcome.Exception,
                "MDS (DDEI) circuit opened for {BreakDelaySeconds}s after status {StatusCode}.",
                breakDelay.TotalSeconds,
                outcome.Result?.StatusCode),
            onReset: () => logger.LogInformation("MDS (DDEI) circuit reset; calls are flowing again."),
            onHalfOpen: () => logger.LogInformation("MDS (DDEI) circuit half-open; testing the next call."));
  }
}