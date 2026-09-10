using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Common.Models.Domain.Dtos;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Telemetry;
using CPS.ComplexCases.FileTransfer.API.Durable.Helpers;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Factories;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using CPS.ComplexCases.FileTransfer.API.Telemetry;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

public class DeleteFiles(ITransferEntityHelper transferEntityHelper, IStorageClientFactory storageClientFactory, ILogger<DeleteFiles> logger, IInitializationHandler initializationHandler, ITelemetryClient telemetryClient)
{
    private readonly ITransferEntityHelper _transferEntityHelper = transferEntityHelper;
    private readonly IStorageClientFactory _storageClientFactory = storageClientFactory;
    private readonly ILogger<DeleteFiles> _logger = logger;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;
    private readonly ITelemetryClient _telemetryClient = telemetryClient;

    [Function(nameof(DeleteFiles))]
    public async Task Run([ActivityTrigger] DeleteFilesPayload? payload, [DurableClient] DurableTaskClient client, CancellationToken cancellationToken = default)
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

        var entity = await _transferEntityHelper.GetTransferEntityAsync(client, payload.TransferId, cancellationToken);

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
            var deletionErrors = BuildDeletionErrors(filesToDelete, result);

            if (deletionErrors.Count != 0)
            {
                _logger.LogWarning(
                    "Failed to delete {FailedCount} of {RequestedCount} files for transfer ID {TransferId}.",
                    deletionErrors.Count,
                    filesToDelete.Count,
                    payload.TransferId);
            }
            else
            {
                _logger.LogInformation("Successfully deleted all files for transfer ID {TransferId}.", payload.TransferId);
            }

            await _transferEntityHelper.DeleteMovedItemsCompleted(client, payload.TransferId, deletionErrors, cancellationToken);

            telemetryEvent.TotalFilesFailedToDelete = deletionErrors.Count;
            telemetryEvent.TotalFilesDeleted = filesToDelete.Count - deletionErrors.Count;
            telemetryEvent.IsSuccessful = deletionErrors.Count == 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting files for transfer ID {TransferId}: {Message}", payload.TransferId, ex.Message);
            telemetryEvent.TotalFilesFailedToDelete = filesToDelete.Count;
            throw;
        }
        finally
        {
            telemetryEvent.DeletionEndTime = DateTime.UtcNow;
            _telemetryClient.TrackEvent(telemetryEvent);
        }
    }

    private static List<DeletionError> BuildDeletionErrors(List<DeletionEntityDto> filesToDelete, DeleteFilesResult result)
    {
        var failedFiles = result.FailedFiles ?? [];
        var deletedFiles = result.DeletedFiles ?? [];

        var deletionErrors = failedFiles.Select(x => new DeletionError
        {
            FileId = x.FileId,
            ErrorMessage = string.IsNullOrEmpty(x.Reason) ? "Unknown error" : x.Reason
        }).ToList();

        var accountedIdentifiers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var deleted in deletedFiles)
        {
            if (!string.IsNullOrEmpty(deleted))
            {
                accountedIdentifiers.Add(deleted);
            }
        }

        foreach (var failed in failedFiles)
        {
            if (!string.IsNullOrEmpty(failed.FileId))
            {
                accountedIdentifiers.Add(failed.FileId);
            }

            if (!string.IsNullOrEmpty(failed.Filename))
            {
                accountedIdentifiers.Add(failed.Filename);
            }
        }

        var unaccounted = filesToDelete.Where(file => !IsAccountedFor(file, accountedIdentifiers)).ToList();
        var missingCount = Math.Max(0, filesToDelete.Count - deletedFiles.Count - failedFiles.Count);

        foreach (var file in unaccounted.Take(missingCount))
        {
            deletionErrors.Add(new DeletionError
            {
                FileId = file.FileId ?? file.Path,
                ErrorMessage = "File was not confirmed deleted by Egress."
            });
        }

        return deletionErrors;
    }

    private static bool IsAccountedFor(DeletionEntityDto file, HashSet<string> accountedIdentifiers)
    {
        if (!string.IsNullOrEmpty(file.FileId) && accountedIdentifiers.Contains(file.FileId))
        {
            return true;
        }

        if (!string.IsNullOrEmpty(file.Path) && accountedIdentifiers.Contains(file.Path))
        {
            return true;
        }

        var fileName = Path.GetFileName(file.Path);
        return !string.IsNullOrEmpty(fileName) && accountedIdentifiers.Contains(fileName);
    }

    private static readonly HashSet<TransferDirection> AllowedDirections =
    [
        TransferDirection.EgressToNetApp
    ];
}
