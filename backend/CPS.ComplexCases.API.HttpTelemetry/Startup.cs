

using CPS.ComplexCases.API.HttpTelemetry;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: WebJobsStartup(typeof(Startup))]

namespace CPS.ComplexCases.API.HttpTelemetry;

public class Startup : IWebJobsStartup2
{
    public void Configure(IWebJobsBuilder builder)
    {
        // wont be called
    }

    public void Configure(WebJobsBuilderContext context, IWebJobsBuilder builder)
    {
        builder.AddExtension<HttpTelemetryExtensionConfigProvider>();
        builder.Services.AddSingleton<ITelemetryInitializer, HttpTelemetryInitializer>();
        builder.Services.AddHttpContextAccessor();
    }
}

[Extension("HttpTelemetry")]
internal class HttpTelemetryExtensionConfigProvider : IExtensionConfigProvider
{
    public void Initialize(ExtensionConfigContext context)
    {
        var bindingRule = context.AddBindingRule<HttpTelemetryAttribute>();
        bindingRule.BindToInput<string?>(attr => "");
    }
}

