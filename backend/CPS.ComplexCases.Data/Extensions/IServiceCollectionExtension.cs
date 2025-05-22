using CPS.ComplexCases.Data.Constants;
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
        options.UseNpgsql("Host=localhost;Port=5432;Database=case_management_datastore;Username=postgres;Password=postgres", x =>
            x.MigrationsHistoryTable("__EFMigrationsHistory", SchemaNames.Lcc)));

    services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
    services.AddScoped<ICaseMetadataRepository, CaseMetadataRepository>();
  }
}