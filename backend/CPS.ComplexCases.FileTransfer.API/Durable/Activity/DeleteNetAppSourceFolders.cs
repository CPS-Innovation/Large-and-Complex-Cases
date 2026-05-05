using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

public class DeleteNetAppSourceFolders(
    INetAppClient netAppClient,
    INetAppArgFactory netAppArgFactory,
    ILogger<DeleteNetAppSourceFolders> logger,
    IInitializationHandler initializationHandler)
{
    private readonly INetAppClient _netAppClient = netAppClient;
    private readonly INetAppArgFactory _netAppArgFactory = netAppArgFactory;
    private readonly ILogger<DeleteNetAppSourceFolders> _logger = logger;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;

    [Function(nameof(DeleteNetAppSourceFolders))]
    public async Task Run([ActivityTrigger] DeleteNetAppSourceFoldersPayload? payload, CancellationToken cancellationToken = default)
    {
        if (payload == null)
            throw new ArgumentNullException(nameof(payload), "DeleteNetAppSourceFoldersPayload cannot be null.");

        _initializationHandler.Initialize(payload.UserName, payload.CorrelationId, payload.CaseId);

        foreach (var folderPath in payload.SourceFolderPaths)
        {
            try
            {
                var arg = _netAppArgFactory.CreateDeleteFileOrFolderArg(
                    payload.BearerToken,
                    payload.BucketName,
                    "DeleteSourceFolder",
                    folderPath,
                    isFolder: true);

                var result = await _netAppClient.DeleteFileOrFolderAsync(arg);

                if (result.Success)
                    _logger.LogInformation(
                        "Deleted source folder {FolderPath} for transfer {TransferId}.",
                        folderPath, payload.TransferId);
                else
                    _logger.LogWarning(
                        "Failed to delete source folder {FolderPath} for transfer {TransferId}. Error: {Error}",
                        folderPath, payload.TransferId, result.ErrorMessage);
            }
            catch (Exception ex)
            {
                // Best-effort: files have already been moved; a failed folder cleanup must not
                // roll back or fail the orchestration.
                _logger.LogError(ex,
                    "Unexpected error deleting source folder {FolderPath} for transfer {TransferId}.",
                    folderPath, payload.TransferId);
            }
        }
    }
}
