
using System.Net;
using System.Net.Http.Headers;
using CPS.ComplexCases.DDEI.Client;
using CPS.ComplexCases.DDEI.Factories;
using CPS.ComplexCases.DDEI.Mappers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Contrib.WaitAndRetry;

namespace CPS.ComplexCases.DDEI.Extensions;

public static class IServiceCollectionExtension
{
  private const string FunctionKey = "x-functions-key";
  private const string DdeiBaseUrlConfigKey = "DdeiBaseUrl";
  private const string DdeiAccessKeyConfigKey = "DdeiAccessKey";
  private const int RetryAttempts = 1;
  private const int FirstRetryDelaySeconds = 1;

  public static void AddDdeiClient(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddTransient<IDdeiArgFactory, DdeiArgFactory>();

    services.AddHttpClient<IDdeiClient, DdeiClient>((service, client) => AddDdeiClient(configuration, client))
      .SetHandlerLifetime(TimeSpan.FromMinutes(5))
      .AddPolicyHandler(GetRetryPolicy());

    services.AddTransient<IDdeiRequestFactory, DdeiRequestFactory>();
    services.AddTransient<ICaseDetailsMapper, CaseDetailsMapper>();
    services.AddTransient<IUserDetailsMapper, UserDetailsMapper>();
  }

  internal static void AddDdeiClient(IConfiguration configuration, HttpClient client)
  {
    var baseUrl = configuration[DdeiBaseUrlConfigKey] ?? throw new ArgumentNullException(DdeiBaseUrlConfigKey);
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add(FunctionKey, configuration[DdeiAccessKeyConfigKey]);
    client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
    client.DefaultRequestHeaders.Add("X-Tunnel-Authorization", "tunnel eyJhbGciOiJFUzI1NiIsImtpZCI6IkM3NDYxNEM5OTE0NjUwNzI2REI1RUZBM0M1OTBDQzdGNjJFOUI4QzQiLCJ0eXAiOiJKV1QifQ.eyJjbHVzdGVySWQiOiJ1a3MxIiwidHVubmVsSWQiOiJzcGlmZnktZG9nLXpoOG4zdHAiLCJzY3AiOiJjb25uZWN0IiwiZXhwIjoxNzQyMDUwNjIzLCJpc3MiOiJodHRwczovL3R1bm5lbHMuYXBpLnZpc3VhbHN0dWRpby5jb20vIiwibmJmIjoxNzQxOTYzMzIzfQ.3CTLONO7u11lq701ersraYgz3t_TOEmgntAXkwB4fa77UnRB_fPZPJMSjgOcLzZdKadEavjXAWhy5_qqLA1NKA");
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
        .HandleResult<HttpResponseMessage>(r => responseStatusCodePredicate(r) && methodPredicate(r))
        .WaitAndRetryAsync(delay);
  }
}