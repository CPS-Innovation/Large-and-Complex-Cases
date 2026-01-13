using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Models.Domain.Dtos;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Telemetry;
using CPS.ComplexCases.FileTransfer.API.Durable.Helpers;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Factories;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using CPS.ComplexCases.FileTransfer.API.Telemetry;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

public class DeleteFiles(ITransferEntityHelper transferEntityHelper, IStorageClientFactory storageClientFactory, ILogger<DeleteFiles> logger, IInitializationHandler initializationHandler, ITelemetryClient telemetryClient)
{
    private readonly ITransferEntityHelper _transferEntityHelper = transferEntityHelper;
    private readonly IStorageClientFactory _storageClientFactory = storageClientFactory;
    private readonly ILogger<DeleteFiles> _logger = logger;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;
    private readonly ITelemetryClient _telemetryClient = telemetryClient;

    [Function(nameof(DeleteFiles))]
    public async Task Run([ActivityTrigger] DeleteFilesPayload? payload, CancellationToken cancellationToken = default)
    {
        _initializationHandler.Initialize(payload?.UserName!, payload?.CorrelationId, payload?.CaseId);

        var telemetryEvent = new FilesDeletedEvent
        {
            TransferId = payload?.TransferId ?? Guid.Empty,
            TransferDirection = payload?.TransferDirection.ToString() ?? string.Empty,
            DeletionStartTime = DateTime.UtcNow
        };

        if (payload == null)
        {
            throw new ArgumentNullException(nameof(payload), "DeleteFilesPayload cannot be null.");
        }

        if (!AllowedDirections.Contains(payload.TransferDirection))
        {
            _logger.LogError("Invalid transfer direction for DeleteFiles activity: {TransferDirection}", payload.TransferDirection);
            throw new ArgumentException("Invalid transfer direction for DeleteFiles activity.", nameof(payload));
        }

        var entity = await _transferEntityHelper.GetTransferEntityAsync(payload.TransferId, cancellationToken);

        if (entity == null)
        {
            _logger.LogError("Transfer entity with ID {TransferId} not found.", payload.TransferId);
            throw new InvalidOperationException($"Transfer entity with ID {payload.TransferId} not found.");
        }

        var filesToDelete = entity.State.SuccessfulItems
            .Where(x => x.Status == TransferItemStatus.Completed)
            .Select(x => new DeletionEntityDto
            {
                Path = x.SourcePath,
                FileId = x.FileId
            })
            .ToList();

        if (filesToDelete.Count == 0)
        {
            _logger.LogInformation("No files to delete for transfer ID {TransferId}.", payload.TransferId);
            return;
        }

        var storageClient = _storageClientFactory.GetSourceClientForDirection(payload.TransferDirection);

        try
        {
            var result = await storageClient.DeleteFilesAsync(filesToDelete, payload.WorkspaceId);

            if (result.FailedFiles != null && result.FailedFiles.Count != 0)
            {
                _logger.LogWarning("Failed to delete some files for transfer ID {TransferId}.", payload.TransferId);

                var failedItems = result.FailedFiles.Select(x => new DeletionError
                {
                    FileId = x.FileId,
                    ErrorMessage = x.Reason ?? "Unknown error"
                }).ToList();

                await _transferEntityHelper.DeleteMovedItemsCompleted(payload.TransferId, failedItems, cancellationToken);
            }
            else
            {
                _logger.LogInformation("Successfully deleted all files for transfer ID {TransferId}.", payload.TransferId);
                await _transferEntityHelper.DeleteMovedItemsCompleted(payload.TransferId, new List<DeletionError>(), cancellationToken);
            }

            telemetryEvent.TotalFilesDeleted = result.DeletedFiles?.Count ?? 0;
            telemetryEvent.TotalFilesFailedToDelete = result.FailedFiles?.Count ?? 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting files for transfer ID {TransferId}: {Message}", payload.TransferId, ex.Message);
        }

        telemetryEvent.DeletionEndTime = DateTime.UtcNow;
        _telemetryClient.TrackEvent(telemetryEvent);
    }

    private static readonly HashSet<TransferDirection> AllowedDirections =
    [
        TransferDirection.EgressToNetApp
    ];
}

