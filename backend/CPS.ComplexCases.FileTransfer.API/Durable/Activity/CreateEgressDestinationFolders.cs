using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Storage;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Factories;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

/// <summary>
/// Pre-creates the distinct Egress destination folders once, before files fan out. This avoids the
/// concurrent create-upload 404 race where many parallel uploads each hit the lazy folder-creation
/// recovery path at the same time and the folder is not yet visible on retry.
/// </summary>
public class CreateEgressDestinationFolders(
    ILogger<CreateEgressDestinationFolders> logger,
    IStorageClientFactory storageClientFactory,
    IInitializationHandler initializationHandler)
{
    private readonly ILogger<CreateEgressDestinationFolders> _logger = logger;
    private readonly IStorageClientFactory _storageClientFactory = storageClientFactory;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;

    [Function(nameof(CreateEgressDestinationFolders))]
    public async Task Run([ActivityTrigger] CreateEgressFoldersPayload payload)
    {
        _initializationHandler.Initialize(payload.UserName!, payload.CorrelationId, payload.CaseId);

        if (payload.FolderPaths.Count == 0)
        {
            _logger.LogInformation("No Egress destination folders to pre-create.");
            return;
        }

        IStorageClient egressClient = _storageClientFactory.GetClient(StorageProvider.Egress);

        // Create sequentially so concurrent overlapping-path creation cannot race.
        foreach (var folderPath in payload.FolderPaths)
        {
            await egressClient.CreateFolderAsync(folderPath, payload.WorkspaceId);
        }

        _logger.LogInformation("Pre-created {FolderCount} Egress destination folder(s).", payload.FolderPaths.Count);
    }
}
