using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;

namespace CPS.ComplexCases.API.HttpTelemetry;

internal static class HealthCheckTelemetryHostRegistration
{
    public static void Register(IServiceCollection services)
    {
        var configDescriptor = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(TelemetryConfiguration));
        if (configDescriptor?.ImplementationFactory is not null)
        {
            var implFactory = configDescriptor.ImplementationFactory;
            services.Remove(configDescriptor);
            services.AddSingleton(provider =>
            {
                var config = (TelemetryConfiguration)implFactory(provider);
                Attach(config);
                return config;
            });
            return;
        }

        if (configDescriptor?.ImplementationInstance is TelemetryConfiguration instance)
        {
            Attach(instance);
            return;
        }

        services.AddSingleton<ITelemetryModule, HealthCheckTelemetryModule>();
    }

    internal static void Attach(TelemetryConfiguration config)
    {
        config.TelemetryProcessorChainBuilder
            .Use(next => new HealthCheckTelemetryFilter(next))
            .Build();
    }
}

internal sealed class HealthCheckTelemetryModule : ITelemetryModule
{
    public void Initialize(TelemetryConfiguration configuration)
    {
        configuration.TelemetryProcessorChainBuilder.Use(next => new HealthCheckTelemetryFilter(next));
    }
}
