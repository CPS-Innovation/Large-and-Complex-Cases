using CPS.ComplexCases.Common.Storage;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;

namespace CPS.ComplexCases.FileTransfer.API.Factories;

public interface IStorageClientFactory
{
    IStorageClient GetClient(StorageProvider provider);
    IStorageClient GetClientForDirection(TransferDirection direction) => GetClientsForDirection(direction).source;
    (IStorageClient source, IStorageClient destination) GetClientsForDirection(TransferDirection direction);
}
