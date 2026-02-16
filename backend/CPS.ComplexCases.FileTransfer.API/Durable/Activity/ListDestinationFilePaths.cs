using CPS.ComplexCases.Common.Storage;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Factories;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using Microsoft.Azure.Functions.Worker;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

public class ListDestinationFilePaths(IStorageClientFactory storageClientFactory)
{
    private readonly IStorageClientFactory _storageClientFactory = storageClientFactory;
    private IStorageClient _egressStorageClient => _storageClientFactory.GetClient(StorageProvider.Egress);

    [Function(nameof(ListDestinationFilePaths))]
    public async Task<HashSet<string>> Run([ActivityTrigger] ListDestinationPayload payload)
    {
        var files = await _egressStorageClient.GetAllFilesFromFolderAsync(
            payload.DestinationPath, payload.WorkspaceId);

        return files
            .Where(f => !string.IsNullOrEmpty(f.FullFilePath))
            .Select(f => f.FullFilePath!)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}