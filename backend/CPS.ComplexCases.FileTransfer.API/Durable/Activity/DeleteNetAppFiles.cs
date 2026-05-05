using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Amazon.S3;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Telemetry;
using CPS.ComplexCases.FileTransfer.API.Durable.Helpers;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using CPS.ComplexCases.FileTransfer.API.Telemetry;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

public class DeleteNetAppFiles(
    ITransferEntityHelper transferEntityHelper,
    INetAppClient netAppClient,
    INetAppArgFactory netAppArgFactory,
    ILogger<DeleteNetAppFiles> logger,
    IInitializationHandler initializationHandler,
    ITelemetryClient telemetryClient)
{
    private readonly ITransferEntityHelper _transferEntityHelper = transferEntityHelper;
    private readonly INetAppClient _netAppClient = netAppClient;
    private readonly INetAppArgFactory _netAppArgFactory = netAppArgFactory;
    private readonly ILogger<DeleteNetAppFiles> _logger = logger;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;
    private readonly ITelemetryClient _telemetryClient = telemetryClient;

    [Function(nameof(DeleteNetAppFiles))]
    public async Task Run([ActivityTrigger] DeleteNetAppFilesPayload? payload, CancellationToken cancellationToken = default)
    {
        _initializationHandler.Initialize(payload?.UserName!, payload?.CorrelationId, payload?.CaseId);

        var telemetryEvent = new FilesDeletedEvent
        {
            TransferId = payload?.TransferId ?? Guid.Empty,
            TransferDirection = TransferDirection.NetAppToNetApp.ToString(),
            DeletionStartTime = DateTime.UtcNow
        };

        if (payload == null)
        {
            throw new ArgumentNullException(nameof(payload), "DeleteNetAppFilesPayload cannot be null.");
        }

        var entity = await _transferEntityHelper.GetTransferEntityAsync(payload.TransferId, cancellationToken);

        if (entity == null)
        {
            _logger.LogError("Transfer entity with ID {TransferId} not found.", payload.TransferId);
            throw new InvalidOperationException($"Transfer entity with ID {payload.TransferId} not found.");
        }

        var filesToDelete = entity.State.SuccessfulItems
            .Where(x => x.Status == TransferItemStatus.Completed)
            .Select(x => x.SourcePath)
            .ToList();

        if (filesToDelete.Count == 0)
        {
            _logger.LogInformation("No files to delete for transfer ID {TransferId}.", payload.TransferId);
            await _transferEntityHelper.DeleteMovedItemsCompleted(payload.TransferId, new List<DeletionError>(), cancellationToken);
            return;
        }

        var failedItems = new List<DeletionError>();
        var deletedCount = 0;

        foreach (var sourcePath in filesToDelete)
        {
            try
            {
                var deleteArg = _netAppArgFactory.CreateDeleteFileOrFolderArg(
                    payload.BearerToken,
                    payload.BucketName,
                    "DeleteObject",
                    sourcePath);

                var result = await _netAppClient.DeleteFileOrFolderAsync(deleteArg);

                if (result.Success)
                {
                    deletedCount++;
                    _logger.LogInformation("Deleted source key {SourcePath} for transfer {TransferId}.", sourcePath, payload.TransferId);
                }
                else
                {
                    _logger.LogWarning("Failed to delete source key {SourcePath} for transfer {TransferId}. Error: {Error}",
                        sourcePath, payload.TransferId, result.ErrorMessage);
                    failedItems.Add(new DeletionError
                    {
                        FileId = sourcePath,
                        ErrorMessage = result.ErrorMessage ?? "Delete returned unsuccessful result."
                    });
                }
            }
            catch (AmazonS3Exception ex) when ((int)ex.StatusCode == 423)
            {
                _logger.LogWarning(ex,
                    "Source file is locked via SMB (423) and could not be deleted after move copy. SourcePath: {SourcePath}, TransferId: {TransferId}.",
                    sourcePath, payload.TransferId);
                failedItems.Add(new DeletionError
                {
                    FileId = sourcePath,
                    ErrorMessage = $"File is locked via SMB and could not be deleted: {sourcePath}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error deleting source key {SourcePath} for transfer {TransferId}.",
                    sourcePath, payload.TransferId);
                failedItems.Add(new DeletionError
                {
                    FileId = sourcePath,
                    ErrorMessage = $"Unexpected error deleting {sourcePath}: {ex.Message}"
                });
            }
        }

        if (failedItems.Count > 0)
        {
            _logger.LogWarning("Failed to delete {FailedCount} of {TotalCount} source files for transfer ID {TransferId}.",
                failedItems.Count, filesToDelete.Count, payload.TransferId);
        }
        else
        {
            _logger.LogInformation("Successfully deleted all {Count} source files for transfer ID {TransferId}.",
                deletedCount, payload.TransferId);
        }

        await _transferEntityHelper.DeleteMovedItemsCompleted(payload.TransferId, failedItems, cancellationToken);

        telemetryEvent.TotalFilesDeleted = deletedCount;
        telemetryEvent.TotalFilesFailedToDelete = failedItems.Count;
        telemetryEvent.DeletionEndTime = DateTime.UtcNow;
        _telemetryClient.TrackEvent(telemetryEvent);
    }
}
