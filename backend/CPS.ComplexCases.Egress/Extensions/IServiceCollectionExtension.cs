using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.Egress.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CPS.ComplexCases.Egress.Extensions;

public static class IServiceCollectionExtension
{
  public static void AddEgressClient(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddTransient<IEgressRequestFactory, EgressRequestFactory>();
    services.AddTransient<IEgressArgFactory, EgressArgFactory>();
    services.Configure<EgressOptions>(configuration.GetSection("EgressOptions"));
    services.AddHttpClient<IEgressClient, EgressClient>(client =>
    {
      var egressServiceUrl = configuration["EgressOptions:Url"];
      if (string.IsNullOrEmpty(egressServiceUrl))
      {
        throw new ArgumentNullException(nameof(egressServiceUrl), "EgressOptions:Url configuration is missing or empty.");
      }
      client.BaseAddress = new Uri(egressServiceUrl);
    });
  }
}