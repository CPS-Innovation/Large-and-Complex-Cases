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

        foreach (var folderSpec in payload.SourceFolders)
        {
            try
            {
                // The folder marker key is the prefix with a trailing slash (S3 convention).
                var folderMarkerKey = folderSpec.FolderPath.EndsWith('/')
                    ? folderSpec.FolderPath
                    : folderSpec.FolderPath + "/";

                var (hasFiles, listingFailed) = await HasAnyFilesAsync(
                    payload.BearerToken, payload.BucketName, folderMarkerKey);

                if (listingFailed)
                {
                    _logger.LogWarning(
                        "Could not verify source folder {FolderPath} is empty before cleanup (listing failed). Skipping deletion for transfer {TransferId}.",
                        folderSpec.FolderPath, payload.TransferId);
                    continue;
                }

                if (hasFiles)
                {
                    _logger.LogWarning(
                        "Source folder {FolderPath} still contains file objects. Skipping folder-marker deletion to avoid data loss. TransferId: {TransferId}.",
                        folderSpec.FolderPath, payload.TransferId);
                    continue;
                }

                // Only the folder marker key is deleted (isFolder: false = non-recursive).
                // Individual source files were already removed by DeleteNetAppFiles, so this
                // only cleans up the zero-byte directory entry.
                var arg = _netAppArgFactory.CreateDeleteFileOrFolderArg(
                    payload.BearerToken,
                    payload.BucketName,
                    "DeleteSourceFolder",
                    folderMarkerKey,
                    isFolder: false);

                var result = await _netAppClient.DeleteFileOrFolderAsync(arg);

                if (result.Success)
                    _logger.LogInformation(
                        "Deleted source folder marker {FolderMarkerKey} for transfer {TransferId}.",
                        folderMarkerKey, payload.TransferId);
                else
                    _logger.LogWarning(
                        "Failed to delete source folder marker {FolderMarkerKey} for transfer {TransferId}. Error: {Error}",
                        folderMarkerKey, payload.TransferId, result.ErrorMessage);
            }
            catch (Exception ex)
            {
                // Best-effort: files have already been moved; a failed folder cleanup must not
                // roll back or fail the orchestration.
                _logger.LogError(ex,
                    "Unexpected error deleting source folder {FolderPath} for transfer {TransferId}.",
                    folderSpec.FolderPath, payload.TransferId);
            }
        }
    }

    /// <summary>
    /// Returns (true, false) if any file objects exist under <paramref name="folderPrefix"/>.
    /// Returns (false, true) when a listing page fails so callers can treat the result as unsafe.
    /// </summary>
    private async Task<(bool HasFiles, bool ListingFailed)> HasAnyFilesAsync(
        string bearerToken, string bucketName, string folderPrefix)
    {
        string? continuationToken = null;

        do
        {
            var arg = _netAppArgFactory.CreateListObjectsInBucketArg(
                bearerToken, bucketName,
                continuationToken: continuationToken,
                prefix: folderPrefix);
            var result = await _netAppClient.ListObjectsInBucketAsync(arg);

            if (result == null) return (false, true);

            if (result.Data.FileData.Any(f => !string.Equals(f.Path, folderPrefix, StringComparison.Ordinal)))
                return (true, false);

            continuationToken = result.Pagination.NextContinuationToken;
        }
        while (!string.IsNullOrEmpty(continuationToken));

        return (false, false);
    }
}
