using CPS.ComplexCases.ActivityLog.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CPS.ComplexCases.ActivityLog.Extensions;

public static class IServiceCollectionExtension
{
    public static void AddActivityLog(this IServiceCollection services)
    {
        services.AddScoped<IActivityLogService, ActivityLogService>();
    }
}