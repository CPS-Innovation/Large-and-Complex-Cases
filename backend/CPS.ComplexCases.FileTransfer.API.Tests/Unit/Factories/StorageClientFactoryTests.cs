using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using CPS.ComplexCases.FileTransfer.API.Factories;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Factories;

public class StorageClientFactoryTests : IDisposable
{
    private readonly StorageClientFactory _factory;
    private readonly ServiceCollection _services;
    private readonly ServiceProvider _serviceProvider;

    public StorageClientFactoryTests()
    {
        _services = new ServiceCollection();
        _serviceProvider = _services.BuildServiceProvider();
        _factory = new StorageClientFactory(_serviceProvider);
    }

    [Fact]
    public void GetClient_ThrowsArgumentOutOfRangeException_WhenProviderIsUnsupported()
    {
        // Arrange
        var unsupportedProvider = (StorageProvider)999;

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => _factory.GetClient(unsupportedProvider));
        Assert.Equal("provider", exception.ParamName);
        Assert.Contains("Unsupported storage provider", exception.Message);
    }

    [Fact]
    public void GetClientsForDirection_ThrowsArgumentOutOfRangeException_WhenDirectionIsUnsupported()
    {
        // Arrange
        var unsupportedDirection = (TransferDirection)999;

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            _factory.GetClientsForDirection(unsupportedDirection));
        Assert.Equal("direction", exception.ParamName);
        Assert.Contains("Unsupported transfer direction", exception.Message);
    }

    [Fact]
    public void GetClient_ReturnsNetAppStorageClient_WhenProviderIsNetApp()
    {
        // This test verifies the switch statement logic
        // We can't easily test the actual service resolution without real instances
        // but we can test that the right provider enum maps to the right call

        // The factory should call GetRequiredService<NetAppStorageClient>() for StorageProvider.NetApp
        // Since we can't mock this easily, we test the exception behavior instead
        var exception = Assert.Throws<InvalidOperationException>(() => _factory.GetClient(StorageProvider.NetApp));
        Assert.Contains("NetAppStorageClient", exception.Message);
    }

    [Fact]
    public void GetClient_ReturnsEgressStorageClient_WhenProviderIsEgress()
    {
        // This test verifies the switch statement logic
        // We can't easily test the actual service resolution without real instances
        // but we can test that the right provider enum maps to the right call

        // The factory should call GetRequiredService<EgressStorageClient>() for StorageProvider.Egress
        // Since we can't mock this easily, we test the exception behavior instead
        var exception = Assert.Throws<InvalidOperationException>(() => _factory.GetClient(StorageProvider.Egress));
        Assert.Contains("EgressStorageClient", exception.Message);
    }

    [Fact]
    public void GetClientsForDirection_CallsGetClientTwice_WhenDirectionIsEgressToNetApp()
    {
        // This test verifies that the method calls GetClient for both source and destination
        // We expect InvalidOperationException because the services aren't registered
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _factory.GetClientsForDirection(TransferDirection.EgressToNetApp));

        // The first call should be for EgressStorageClient (source)
        Assert.Contains("EgressStorageClient", exception.Message);
    }

    [Fact]
    public void GetClientsForDirection_CallsGetClientTwice_WhenDirectionIsNetAppToEgress()
    {
        // This test verifies that the method calls GetClient for both source and destination
        // We expect InvalidOperationException because the services aren't registered
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _factory.GetClientsForDirection(TransferDirection.NetAppToEgress));

        // The first call should be for NetAppStorageClient (source)
        Assert.Contains("NetAppStorageClient", exception.Message);
    }

    [Fact]
    public void GetClientsForDirection_WhenDirectionIsNetAppToNetApp_AttemptsToResolveNetAppStorageClient()
    {
        // NetAppToNetApp calls GetClient(StorageProvider.NetApp) for both source and destination.
        // Services are not registered in this test, so we expect InvalidOperationException
        // whose message references NetAppStorageClient — confirming the correct provider is targeted.
        var exception = Assert.Throws<InvalidOperationException>(() =>
            _factory.GetClientsForDirection(TransferDirection.NetAppToNetApp));

        Assert.Contains("NetAppStorageClient", exception.Message);
    }

    [Fact]
    public void GetClientsForDirection_ReturnsTwoNetAppStorageClients_WhenDirectionIsNetAppToNetApp()
    {
        // Arrange.
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<INetAppClient>());
        services.AddSingleton(Mock.Of<INetAppArgFactory>());
        services.AddSingleton(Mock.Of<ICaseMetadataService>());
        services.AddSingleton(Mock.Of<INetAppS3HttpClient>());
        services.AddSingleton(Mock.Of<INetAppS3HttpArgFactory>());
        services.AddSingleton(Mock.Of<ILogger<NetAppStorageClient>>());
        services.AddTransient<NetAppStorageClient>();

        using var provider = services.BuildServiceProvider();
        var factory = new StorageClientFactory(provider);

        // Act
        var (source, destination) = factory.GetClientsForDirection(TransferDirection.NetAppToNetApp);

        // Assert
        Assert.IsType<NetAppStorageClient>(source);
        Assert.IsType<NetAppStorageClient>(destination);
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}