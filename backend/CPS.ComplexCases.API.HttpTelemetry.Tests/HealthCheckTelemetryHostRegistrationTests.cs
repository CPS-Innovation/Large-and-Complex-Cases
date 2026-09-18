using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;

namespace CPS.ComplexCases.API.HttpTelemetry.Tests;

public class HealthCheckTelemetryHostRegistrationTests
{
    [Fact]
    public void Register_WrapsTelemetryConfiguration_AndDropsStatusHostTelemetry()
    {
        var channel = new StubChannel();
        var services = new ServiceCollection();
        services.AddSingleton(_ => new TelemetryConfiguration { TelemetryChannel = channel });

        HealthCheckTelemetryHostRegistration.Register(services);

        var config = services.BuildServiceProvider().GetRequiredService<TelemetryConfiguration>();
        var client = new TelemetryClient(config);

        var statusRequest = new RequestTelemetry
        {
            Name = "Status",
            Url = new Uri("https://example.com/api/status")
        };
        var invokeDependency = new DependencyTelemetry
        {
            Name = "Invoke",
            Type = "InProc"
        };
        invokeDependency.Context.Operation.Name = "Functions.Status";

        client.TrackRequest(statusRequest);
        client.Track(invokeDependency);
        client.TrackRequest(new RequestTelemetry
        {
            Name = "GetCase",
            Url = new Uri("https://example.com/api/v1/cases/1")
        });
        client.Flush();

        var remaining = Assert.Single(channel.Items);
        var remainingRequest = Assert.IsType<RequestTelemetry>(remaining);
        Assert.Equal("GetCase", remainingRequest.Name);
    }

    [Fact]
    public void Register_AddsTelemetryModule_WhenConfigurationIsNotRegistered()
    {
        var services = new ServiceCollection();

        HealthCheckTelemetryHostRegistration.Register(services);

        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(ITelemetryModule)
            && descriptor.ImplementationType == typeof(HealthCheckTelemetryModule));
    }

    [Fact]
    public void Module_Initialize_DropsStatusRequestsAfterBuild()
    {
        var channel = new StubChannel();
        var config = new TelemetryConfiguration { TelemetryChannel = channel };

        new HealthCheckTelemetryModule().Initialize(config);
        config.TelemetryProcessorChainBuilder.Build();

        var client = new TelemetryClient(config);
        client.TrackRequest(new RequestTelemetry
        {
            Name = "Status",
            Url = new Uri("https://example.com/api/status")
        });
        client.TrackRequest(new RequestTelemetry
        {
            Name = "GetCase",
            Url = new Uri("https://example.com/api/v1/cases/1")
        });
        client.Flush();

        var remaining = Assert.Single(channel.Items);
        var remainingRequest = Assert.IsType<RequestTelemetry>(remaining);
        Assert.Equal("GetCase", remainingRequest.Name);
    }

    private sealed class StubChannel : ITelemetryChannel
    {
        public List<ITelemetry> Items { get; } = [];
        public bool? DeveloperMode { get; set; }
        public string? EndpointAddress { get; set; }
        public void Send(ITelemetry item) => Items.Add(item);
        public void Flush() { }
        public void Dispose() { }
    }
}
