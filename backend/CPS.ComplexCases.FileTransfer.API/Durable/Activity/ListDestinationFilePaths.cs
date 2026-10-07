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

    // Returns each destination file path with the size Egress reports for it, or null where the
    // listing carries no size. The post-transfer verification pass needs the size to tell a
    // committed file apart from the placeholder a failed commit leaves behind.
    [Function(nameof(ListDestinationFilePaths))]
    public async Task<Dictionary<string, long?>> Run([ActivityTrigger] ListDestinationPayload payload)
    {
        var files = await _egressStorageClient.GetAllFilesFromFolderAsync(
            payload.DestinationPath, payload.WorkspaceId);

        var filesByPath = new Dictionary<string, long?>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in files.Where(f => !string.IsNullOrEmpty(f.FullFilePath)))
        {
            filesByPath[file.FullFilePath!] = file.FileSizeBytes;
        }

        return filesByPath;
    }
}
