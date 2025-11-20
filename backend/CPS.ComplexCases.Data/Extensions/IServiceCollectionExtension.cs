using Azure.Core;
using Azure.Identity;
using CPS.ComplexCases.Data.Constants;
using CPS.ComplexCases.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace CPS.ComplexCases.Data.Extensions;

public static class IServiceCollectionExtension
{
  public static void AddDataClient(this IServiceCollection services, IConfiguration configuration)
  {
    var authMode = configuration["Postgres:AuthMode"];
    var connectionString = configuration.GetConnectionString("CaseManagementDatastoreConnection");

    if (authMode?.Equals("AAD", StringComparison.OrdinalIgnoreCase) == true)
    {
      // Azure AD authentication with token refresh
      var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

      if (dataSourceBuilder.ConnectionStringBuilder.Password is null) // allows password override locally
      {
        var credentials = new DefaultAzureCredential();
        dataSourceBuilder.UsePeriodicPasswordProvider(
            async (_, ct) =>
            {
              // This specific context must be used to get a token to access the postgres database
              var token = await credentials.GetTokenAsync(
                          new TokenRequestContext(["https://ossrdbms-aad.database.windows.net/.default"]),
                          ct).ConfigureAwait(false);
              return token.Token;
            },
            TimeSpan.FromHours(1), // successRefreshInterval - gets a new token every hour
            TimeSpan.FromSeconds(30) // failureRefreshInterval - retries after 30 seconds if a token retrieval fails
        );
      }

      var dataSource = dataSourceBuilder.Build();
      services.AddDbContext<ApplicationDbContext>(options =>
          options.UseNpgsql(dataSource, x =>
              x.MigrationsHistoryTable("__EFMigrationsHistory", SchemaNames.Lcc)));
    }
    else
    {
      // Password-based authentication (local/dev mode)
      services.AddDbContext<ApplicationDbContext>(options =>
          options.UseNpgsql(connectionString, x =>
              x.MigrationsHistoryTable("__EFMigrationsHistory", SchemaNames.Lcc)));
    }

    services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
    services.AddScoped<ICaseMetadataRepository, CaseMetadataRepository>();
  }
}