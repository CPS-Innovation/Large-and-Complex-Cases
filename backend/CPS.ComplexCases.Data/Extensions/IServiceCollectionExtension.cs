using CPS.ComplexCases.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CPS.ComplexCases.Data.Extensions;

public static class IServiceCollectionExtension
{
  public static void AddDataClient(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(configuration.GetConnectionString("CaseManagementDatastoreConnection")));

    services.AddScoped<ICaseMetadataRepository, CaseMetadataRepository>();
  }
}