
using CPS.ComplexCases.DDEI.Client;
using CPS.ComplexCases.DDEI.Factories;
using CPS.ComplexCases.DDEI.Tactical.Client;
using CPS.ComplexCases.DDEI.Tactical.Factories;
using CPS.ComplexCases.DDEI.Tactical.Mappers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CPS.ComplexCases.DDEI.Tactical.Extensions;

public static class IServiceCollectionExtension
{
  public static void AddDdeiClientTactical(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddSingleton<IDdeiRequestFactoryTactical, DdeiRequestFactoryTactical>();
    services.AddSingleton<IAuthenticationResponseMapper, AuthenticationResponseMapper>();
    services.AddHttpClient<IDdeiClientTactical, DdeiClient>((service, client) =>
      DDEI.Extensions.IServiceCollectionExtension.AddDdeiClient(configuration, client));
  }
}
