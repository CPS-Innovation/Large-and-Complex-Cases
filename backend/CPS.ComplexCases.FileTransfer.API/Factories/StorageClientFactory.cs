
using CPS.ComplexCases.Common.Interfaces;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace CPS.ComplexCases.FileTransfer.API.Factories;

public class StorageClientFactory(IServiceProvider serviceProvider) : IStorageClientFactory
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public IStorageClient GetClient(StorageProvider provider)
    {
        return provider switch
        {
            // to do change to NetApp when implemented
            StorageProvider.NetApp => _serviceProvider.GetRequiredService<EgressStorageClient>(),
            StorageProvider.Egress => _serviceProvider.GetRequiredService<EgressStorageClient>(),
            _ => throw new ArgumentOutOfRangeException(nameof(provider), $"Unsupported storage provider: {provider}")
        };
    }
    public (IStorageClient source, IStorageClient destination) GetClientsForDirection(TransferDirection direction)
    {
        return direction switch
        {
            TransferDirection.EgressToNetApp => (
                GetClient(StorageProvider.Egress),
                GetClient(StorageProvider.NetApp)
            ),
            TransferDirection.NetAppToEgress => (
                GetClient(StorageProvider.NetApp),
                GetClient(StorageProvider.Egress)
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), $"Unsupported transfer direction: {direction}")
        };
    }
}