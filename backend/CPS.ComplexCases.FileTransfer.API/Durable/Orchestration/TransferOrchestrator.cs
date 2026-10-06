using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.Common.Telemetry;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.FileTransfer.API.Durable.Activity;
using CPS.ComplexCases.FileTransfer.API.Durable.Helpers;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using CPS.ComplexCases.FileTransfer.API.Models.Configuration;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using CPS.ComplexCases.FileTransfer.API.Telemetry;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Orchestration;

public class TransferOrchestrator(IOptions<SizeConfig> sizeConfig, ITelemetryClient telemetryClient, IInitializationHandler initializationHandler)
{
    private readonly SizeConfig _sizeConfig = sizeConfig.Value;
    private readonly ITelemetryClient _telemetryClient = telemetryClient;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;

    internal const string MissingFromDestinationMessage =
        "The destination service reported the file as uploaded but it is not present at the destination. Please try again.";

    [Function(nameof(TransferOrchestrator))]
    public async Task RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        ILogger logger = context.CreateReplaySafeLogger(nameof(TransferOrchestrator));
        logger.LogInformation("TransferOrchestrator started.");

        var input = context.GetInput<TransferPayload>();
        if (input == null)
        {
            logger.LogError("TransferOrchestrator input is null.");
            throw new ArgumentNullException(nameof(input));
        }

        _initializationHandler.Initialize(input.UserName!, input.CorrelationId);

        var transferOrchestrationEvent = new TransferOrchestrationEvent
        {
            TransferDirection = input.TransferDirection.ToString(),
            TotalFiles = input.SourcePaths.Count,
            BucketName = input.BucketName,
            CaseId = input.CaseId,
            OrchestrationStartTime = context.CurrentUtcDateTime
        };

        var filesProcessingStarted = false;
        try
        {
            var (entityId, transferEntity) = await InitializeTransferEntityAsync(context, input, logger);

            var cleanFiles = await FilterDuplicateDestinationFilesAsync(context, input, entityId);

            cleanFiles = await ValidateSourceFilesAsync(context, input, entityId, cleanFiles, logger);

            await PreCreateEgressDestinationFoldersAsync(context, input, cleanFiles, logger);

            filesProcessingStarted = true;

            var allResults = await FanOutTransferFilesAsync(
                context, input, transferEntity, cleanFiles, entityId, transferOrchestrationEvent, logger);

            await VerifyEgressDestinationAsync(
                context, input, cleanFiles, entityId, allResults, transferOrchestrationEvent, logger);

            var retriesAttempted = await RetryTransientFailuresAsync(
                context, input, transferEntity, cleanFiles, entityId, allResults, transferOrchestrationEvent, logger);

            // Retried files were acknowledged by Egress the same way the first pass was, so they need
            // the same check. Anything still missing has no retries left and stays failed, which makes
            // FinalizeTransfer report PartiallyCompleted rather than a false Completed.
            if (retriesAttempted)
            {
                await VerifyEgressDestinationAsync(
                    context, input, cleanFiles, entityId, allResults, transferOrchestrationEvent, logger);
            }

            await DeleteSourceFilesIfMoveAsync(context, input);

            await FinalizeAndLogCompletionAsync(context, input, logger);

            transferOrchestrationEvent.IsSuccessful = transferOrchestrationEvent.TotalFilesFailed == 0;
        }
        catch (Exception ex)
        {
            await HandleOrchestratorFailureAsync(context, input, logger, ex, filesProcessingStarted);
            throw;
        }
        finally
        {
            transferOrchestrationEvent.OrchestrationEndTime = context.CurrentUtcDateTime;
            _telemetryClient.TrackEvent(transferOrchestrationEvent);
        }
    }

    private static async Task<(EntityInstanceId EntityId, TransferEntity TransferEntity)> InitializeTransferEntityAsync(
        TaskOrchestrationContext context,
        TransferPayload input,
        ILogger logger)
    {
        var transferEntity = new TransferEntity
        {
            Id = input.TransferId,
            Status = TransferStatus.Initiated,
            DestinationPath = input.DestinationPath,
            SourcePaths = input.SourcePaths,
            SourceRootFolderPath = input.SourceRootFolderPath,
            CaseId = input.CaseId,
            TransferType = input.TransferType,
            Direction = input.TransferDirection,
            TotalFiles = input.SourcePaths.Count,
            IsRetry = input.IsRetry ?? false,
            UserName = input.UserName,
            CorrelationId = input.CorrelationId,
            BearerToken = input.BearerToken
        };

        var entityId = new EntityInstanceId(nameof(TransferEntityState), input.TransferId.ToString());

        await context.Entities.CallEntityAsync(
            entityId,
            nameof(TransferEntityState.Initialize),
            transferEntity);

        await TryCallUpdateActivityLogAsync(
            context,
            logger,
            new UpdateActivityLogPayload
            {
                ActionType = ActivityLog.Enums.ActionType.TransferInitiated,
                TransferId = input.TransferId.ToString(),
                UserName = input.UserName,
                CorrelationId = input.CorrelationId
            });

        return (entityId, transferEntity);
    }

    private async Task<List<TransferSourcePath>> FilterDuplicateDestinationFilesAsync(
        TaskOrchestrationContext context,
        TransferPayload input,
        EntityInstanceId entityId)
    {
        if (input.TransferDirection != TransferDirection.NetAppToEgress)
        {
            return input.SourcePaths;
        }

        var destinationFiles = await context.CallActivityAsync<HashSet<string>>(
            nameof(ListDestinationFilePaths),
            new ListDestinationPayload(input.WorkspaceId, input.DestinationPath));

        var (duplicates, cleanFiles) = PartitionByDestinationCollision(
            input.SourcePaths, input.DestinationPath, input.SourceRootFolderPath, destinationFiles);

        foreach (var (sourcePath, destPath) in duplicates)
        {
            await context.Entities.CallEntityAsync(
                entityId,
                nameof(TransferEntityState.AddFailedItem),
                new TransferFailedItem
                {
                    SourcePath = sourcePath.Path,
                    ErrorCode = TransferErrorCode.FileExists,
                    ErrorMessage = $"File already exists at destination: {destPath}"
                });

            LogFileConflictTelemetry(input.CaseId, sourcePath.Path, destPath, input.TransferDirection, input.TransferId);
        }

        return cleanFiles;
    }

    private async Task<List<TransferSourcePath>> ValidateSourceFilesAsync(
        TaskOrchestrationContext context,
        TransferPayload input,
        EntityInstanceId entityId,
        List<TransferSourcePath> cleanFiles,
        ILogger logger)
    {
        if (cleanFiles.Count == 0)
        {
            return cleanFiles;
        }

        var available = new List<TransferSourcePath>();
        var remaining = cleanFiles;
        var maxAttempts = Math.Max(1, _sizeConfig.SourceValidationRetryAttempts);
        var intervalSeconds = Math.Max(1, _sizeConfig.SourceValidationRetryIntervalSeconds);

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            var result = await context.CallActivityAsync<ValidateSourceFilesResult>(
                nameof(ValidateSourceFiles),
                new ValidateSourceFilesPayload
                {
                    TransferDirection = input.TransferDirection,
                    SourcePaths = remaining,
                    WorkspaceId = input.WorkspaceId,
                    BearerToken = input.BearerToken,
                    BucketName = input.BucketName,
                    CaseId = input.CaseId,
                    UserName = input.UserName,
                    CorrelationId = input.CorrelationId
                },
                new TaskOptions(TaskRetryOptions.FromRetryPolicy(new RetryPolicy(
                    maxNumberOfAttempts: _sizeConfig.FolderPreCreateRetryAttempts,
                    firstRetryInterval: TimeSpan.FromSeconds(_sizeConfig.FolderPreCreateFirstRetryIntervalSeconds),
                    backoffCoefficient: _sizeConfig.FolderPreCreateBackoffCoefficient)))) ?? new ValidateSourceFilesResult();

            available.AddRange(result.Available ?? []);

            foreach (var failedItem in result.Failed ?? [])
            {
                await context.Entities.CallEntityAsync(
                    entityId,
                    nameof(TransferEntityState.AddFailedItem),
                    failedItem);
            }

            remaining = result.Missing ?? [];
            if (remaining.Count == 0)
            {
                break;
            }

            if (attempt < maxAttempts - 1)
            {
                logger.LogInformation(
                    "Waiting {IntervalSeconds}s for {Count} source file(s) to become available (attempt {Attempt}/{MaxAttempts}) for TransferId {TransferId}.",
                    intervalSeconds, remaining.Count, attempt + 1, maxAttempts, input.TransferId);

                var nextCheckAt = context.CurrentUtcDateTime.AddSeconds(intervalSeconds);
                await context.CreateTimer(nextCheckAt, CancellationToken.None);
            }
        }

        foreach (var missing in remaining)
        {
            await context.Entities.CallEntityAsync(
                entityId,
                nameof(TransferEntityState.AddFailedItem),
                new TransferFailedItem
                {
                    SourcePath = missing.Path,
                    ErrorCode = TransferErrorCode.SourceFileNotFound,
                    ErrorMessage = TransferErrorMessages.GetUserMessage(TransferErrorCode.SourceFileNotFound)
                });
        }

        return available;
    }

    internal static (List<(TransferSourcePath Source, string DestPath)> Duplicates, List<TransferSourcePath> CleanFiles)
        PartitionByDestinationCollision(
            List<TransferSourcePath> sourcePaths,
            string destinationPath,
            string? sourceRootFolderPath,
            HashSet<string> destinationFiles)
    {
        var duplicates = new List<(TransferSourcePath Source, string DestPath)>();
        var cleanFiles = new List<TransferSourcePath>();

        foreach (var sourcePath in sourcePaths)
        {
            var destPath = GetEgressDestinationPath(destinationPath, sourcePath.RelativePath, sourceRootFolderPath);
            if (destinationFiles.Contains(destPath))
            {
                duplicates.Add((sourcePath, destPath));
            }
            else
            {
                cleanFiles.Add(sourcePath);
            }
        }

        return (duplicates, cleanFiles);
    }

    private async Task PreCreateEgressDestinationFoldersAsync(
        TaskOrchestrationContext context,
        TransferPayload input,
        List<TransferSourcePath> cleanFiles,
        ILogger logger)
    {
        if (input.TransferDirection != TransferDirection.NetAppToEgress || cleanFiles.Count == 0)
        {
            return;
        }

        var destinationFolderPaths = GetDistinctDestinationFolderPaths(
            cleanFiles, input.DestinationPath, input.SourceRootFolderPath);

        if (destinationFolderPaths.Count == 0)
        {
            return;
        }

        try
        {
            await context.CallActivityAsync(
                nameof(CreateEgressDestinationFolders),
                new CreateEgressFoldersPayload
                {
                    WorkspaceId = input.WorkspaceId!,
                    FolderPaths = destinationFolderPaths,
                    CaseId = input.CaseId,
                    UserName = input.UserName,
                    CorrelationId = input.CorrelationId
                },
                new TaskOptions(TaskRetryOptions.FromRetryPolicy(new RetryPolicy(
                    maxNumberOfAttempts: _sizeConfig.FolderPreCreateRetryAttempts,
                    firstRetryInterval: TimeSpan.FromSeconds(_sizeConfig.FolderPreCreateFirstRetryIntervalSeconds),
                    backoffCoefficient: _sizeConfig.FolderPreCreateBackoffCoefficient))));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Pre-creation of Egress destination folders failed after retries for TransferId {TransferId}; " +
                "continuing so files fall back to the lazy folder-creation path during upload.",
                input.TransferId);
        }
    }

    internal static List<string> GetDistinctDestinationFolderPaths(
        List<TransferSourcePath> cleanFiles,
        string destinationPath,
        string? sourceRootFolderPath) =>
        cleanFiles
            .Select(sp => EgressStorageClient.GetDestinationFolderPath(destinationPath, sp.RelativePath, sourceRootFolderPath))
            .Where(p => !string.IsNullOrEmpty(p))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private async Task<List<TransferResult>> FanOutTransferFilesAsync(
        TaskOrchestrationContext context,
        TransferPayload input,
        TransferEntity transferEntity,
        List<TransferSourcePath> cleanFiles,
        EntityInstanceId entityId,
        TransferOrchestrationEvent transferOrchestrationEvent,
        ILogger logger)
    {
        await context.CallActivityAsync(
            nameof(UpdateTransferStatus),
            new UpdateTransferStatusPayload
            {
                TransferId = input.TransferId,
                Status = TransferStatus.InProgress,
            });

        var batches = SizeTieredBatcher.BuildBatches(
            cleanFiles,
            _sizeConfig.MinMultipartSizeBytes,
            _sizeConfig.BatchSize,
            _sizeConfig.LargeFileBatchSize);

        var allResults = new List<TransferResult>();

        foreach (var batch in batches)
        {
            logger.LogInformation(
                "Fanning out {Count} {Tier} file(s) for TransferId {TransferId}.",
                batch.Files.Length,
                batch.IsLargeFileTier ? "large (multipart)" : "small (single-upload)",
                input.TransferId);

            var batchResults = await Task.WhenAll(batch.Files.Select(sourcePath =>
                context.CallActivityAsync<TransferResult>(
                    nameof(TransferFile),
                    BuildTransferFilePayload(input, transferEntity, sourcePath))));

            await TransferResultProcessor.ProcessAsync(context, entityId, batchResults, transferOrchestrationEvent);
            allResults.AddRange(batchResults);
        }

        return allResults;
    }

    // Egress acknowledges an upload with HTTP 200 before the file is committed, and under a chunk
    // race it can acknowledge an upload that never materialises. A single destination listing after
    // the fan-out catches those files so they are retried rather than silently reported as
    // transferred. One listing is used rather than a per-file probe, which would fan out a folder
    // listing per file and overwhelm Egress.
    private async Task VerifyEgressDestinationAsync(
        TaskOrchestrationContext context,
        TransferPayload input,
        List<TransferSourcePath> cleanFiles,
        EntityInstanceId entityId,
        List<TransferResult> allResults,
        TransferOrchestrationEvent transferOrchestrationEvent,
        ILogger logger)
    {
        if (input.TransferDirection != TransferDirection.NetAppToEgress)
        {
            return;
        }

        var successfulPaths = allResults
            .Where(r => r != null && r.IsSuccess && r.SuccessfulItem != null)
            .Select(r => r.SuccessfulItem!.SourcePath)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var transferredSources = cleanFiles
            .Where(f => successfulPaths.Contains(f.FullFilePath ?? f.Path))
            .ToList();

        if (transferredSources.Count == 0)
        {
            return;
        }

        // Egress commits uploads asynchronously, so a listing taken as soon as the last completion
        // returns can miss files that are still landing.
        var settleDelaySeconds = Math.Max(0, _sizeConfig.EgressVerificationSettleDelaySeconds);
        if (settleDelaySeconds > 0)
        {
            var verifyAt = context.CurrentUtcDateTime.AddSeconds(settleDelaySeconds);
            await context.CreateTimer(verifyAt, CancellationToken.None);
        }

        var landedFiles = await context.CallActivityAsync<HashSet<string>>(
            nameof(ListDestinationFilePaths),
            new ListDestinationPayload(input.WorkspaceId, input.DestinationPath));

        // Reuses the pre-flight collision logic in reverse: a "collision" against the destination
        // listing means the file landed, so the clean partition is what is missing.
        var (_, missing) = PartitionByDestinationCollision(
            transferredSources, input.DestinationPath, input.SourceRootFolderPath, landedFiles);

        if (missing.Count == 0)
        {
            return;
        }

        var missingSourcePaths = new List<string>();

        foreach (var source in missing)
        {
            var sourceIdentifier = source.FullFilePath ?? source.Path;
            var result = allResults.FirstOrDefault(r =>
                r != null
                && r.IsSuccess
                && r.SuccessfulItem != null
                && string.Equals(r.SuccessfulItem.SourcePath, sourceIdentifier, StringComparison.OrdinalIgnoreCase));

            if (result == null)
            {
                continue;
            }

            logger.LogWarning(
                "Egress reported a successful upload for {SourcePath} but the file is not present at {DestinationPath} for TransferId {TransferId}. Marking it as a transient failure so it is re-attempted.",
                sourceIdentifier,
                GetEgressDestinationPath(input.DestinationPath, source.RelativePath, input.SourceRootFolderPath),
                input.TransferId);

            transferOrchestrationEvent.TotalFilesTransferred--;
            transferOrchestrationEvent.TotalBytesTransferred -= result.SuccessfulItem!.Size;
            transferOrchestrationEvent.TotalFilesFailed++;

            result.IsSuccess = false;
            result.SuccessfulItem = null;
            result.FailedItem = new TransferFailedItem
            {
                SourcePath = sourceIdentifier,
                Status = TransferItemStatus.Failed,
                ErrorCode = TransferErrorCode.Transient,
                ErrorMessage = MissingFromDestinationMessage
            };

            missingSourcePaths.Add(sourceIdentifier);
        }

        if (missingSourcePaths.Count == 0)
        {
            return;
        }

        await context.Entities.CallEntityAsync(
            entityId,
            nameof(TransferEntityState.DemoteSuccessfulItems),
            new DemoteSuccessfulItemsPayload(missingSourcePaths, MissingFromDestinationMessage));
    }

    private async Task<bool> RetryTransientFailuresAsync(
        TaskOrchestrationContext context,
        TransferPayload input,
        TransferEntity transferEntity,
        List<TransferSourcePath> cleanFiles,
        EntityInstanceId entityId,
        List<TransferResult> allResults,
        TransferOrchestrationEvent transferOrchestrationEvent,
        ILogger logger)
    {
        int maxOrchestratorRetries = _sizeConfig.MaxOrchestratorRetries;
        var retryStateWritten = false;

        try
        {
            for (int attempt = 0; attempt < maxOrchestratorRetries; attempt++)
            {
                var retryableFailures = allResults
                    .Where(r => r != null && !r.IsSuccess && r.FailedItem?.ErrorCode == TransferErrorCode.Transient)
                    .ToList();

                if (retryableFailures.Count == 0) break;

                logger.LogWarning(
                    "Orchestrator retry attempt {Attempt}/{MaxRetries}: re-attempting {Count} transiently failed files.",
                    attempt + 1, maxOrchestratorRetries, retryableFailures.Count);

                var delaySeconds = (int)(60 * Math.Pow(2, attempt));
                var nextRetryAt = context.CurrentUtcDateTime.AddSeconds(delaySeconds);

                await TransferRetryStateNotifier.WaitingForRetryAsync(
                    context, entityId, attempt + 1, maxOrchestratorRetries, retryableFailures.Count, delaySeconds, nextRetryAt);
                retryStateWritten = true;

                await context.CreateTimer(nextRetryAt, CancellationToken.None);

                await TransferRetryStateNotifier.RetryInProgressAsync(
                    context, entityId, attempt + 1, maxOrchestratorRetries, retryableFailures.Count, delaySeconds);

                var failedPaths = retryableFailures
                    .Select(r => r.FailedItem!.SourcePath)
                    .ToHashSet();

                var retrySourcePaths = cleanFiles
                    .Where(f => failedPaths.Contains(f.Path) || (f.FullFilePath != null && failedPaths.Contains(f.FullFilePath)))
                    .ToList();

                await context.Entities.CallEntityAsync(
                    entityId,
                    nameof(TransferEntityState.RemoveTransientFailures));

                transferOrchestrationEvent.TotalFilesFailed -= retryableFailures.Count;

                int retryBatchSize = Math.Max(1, _sizeConfig.RetryBatchSize);
                var retryResults = new List<TransferResult>();

                for (int i = 0; i < retrySourcePaths.Count; i += retryBatchSize)
                {
                    var chunk = retrySourcePaths.Skip(i).Take(retryBatchSize);
                    var retryBatch = chunk.Select(sp =>
                        context.CallActivityAsync<TransferResult>(
                            nameof(TransferFile),
                            BuildTransferFilePayload(input, transferEntity, sp))).ToList();

                    var batchResults = await Task.WhenAll(retryBatch);
                    await TransferResultProcessor.ProcessAsync(context, entityId, batchResults, transferOrchestrationEvent, isRetry: true);
                    retryResults.AddRange(batchResults);
                }

                allResults.RemoveAll(r => r != null && !r.IsSuccess && r.FailedItem?.ErrorCode == TransferErrorCode.Transient);
                allResults.AddRange(retryResults);
            }
        }
        finally
        {
            if (retryStateWritten)
            {
                await TransferRetryStateNotifier.ClearAsync(context, entityId);
            }
        }

        return retryStateWritten;
    }

    private static async Task DeleteSourceFilesIfMoveAsync(TaskOrchestrationContext context, TransferPayload input)
    {
        if (input.TransferDirection != TransferDirection.EgressToNetApp || input.TransferType != TransferType.Move)
        {
            return;
        }

        await context.CallActivityAsync(
            nameof(DeleteFiles),
            new DeleteFilesPayload
            {
                TransferId = input.TransferId,
                TransferDirection = input.TransferDirection,
                WorkspaceId = input.WorkspaceId,
                UserName = input.UserName!,
                CorrelationId = input.CorrelationId,
                CaseId = input.CaseId
            });
    }

    private static async Task FinalizeAndLogCompletionAsync(TaskOrchestrationContext context, TransferPayload input, ILogger logger)
    {
        await context.CallActivityAsync(
            nameof(FinalizeTransfer),
            new FinalizeTransferPayload
            {
                TransferId = input.TransferId,
            });

        await TryCallUpdateActivityLogAsync(
            context,
            logger,
            new UpdateActivityLogPayload
            {
                ActionType = ActivityLog.Enums.ActionType.TransferCompleted,
                TransferId = input.TransferId.ToString(),
                UserName = input.UserName,
                CorrelationId = input.CorrelationId
            });
    }

    private static async Task HandleOrchestratorFailureAsync(
        TaskOrchestrationContext context,
        TransferPayload input,
        ILogger logger,
        Exception ex,
        bool filesProcessingStarted)
    {
        logger.LogError(ex, "TransferOrchestrator failed for TransferId: {TransferId}. With CorrelationId {CorrelationId}", input.TransferId, input.CorrelationId);

        var errorMessage = filesProcessingStarted
            ? ex.Message
            : $"The transfer failed before any files were processed. {ex.Message}";

        await context.CallActivityAsync(
            nameof(UpdateTransferStatus),
            new UpdateTransferStatusPayload
            {
                TransferId = input.TransferId,
                Status = TransferStatus.Failed,
                ErrorMessage = errorMessage,
            });

        await TryCallUpdateActivityLogAsync(
            context,
            logger,
            new UpdateActivityLogPayload
            {
                ActionType = ActivityLog.Enums.ActionType.TransferFailed,
                TransferId = input.TransferId.ToString(),
                UserName = input.UserName,
                CorrelationId = input.CorrelationId,
                ExceptionMessage = errorMessage
            });
    }

    private static async Task TryCallUpdateActivityLogAsync(
        TaskOrchestrationContext context,
        ILogger logger,
        UpdateActivityLogPayload payload)
    {
        try
        {
            await context.CallActivityAsync(nameof(UpdateActivityLog), payload);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "UpdateActivityLog failed for TransferId {TransferId} ({ActionType}); continuing transfer.",
                payload.TransferId,
                payload.ActionType);
        }
    }

    private static TransferFilePayload BuildTransferFilePayload(
        TransferPayload input,
        TransferEntity transferEntity,
        TransferSourcePath sourcePath) =>
        new()
        {
            CaseId = input.CaseId,
            SourcePath = sourcePath,
            DestinationPath = transferEntity.DestinationPath,
            TransferId = transferEntity.Id,
            TransferType = transferEntity.TransferType,
            TransferDirection = transferEntity.Direction,
            WorkspaceId = input.WorkspaceId,
            SourceRootFolderPath = input.SourceRootFolderPath,
            BearerToken = input.BearerToken,
            BucketName = input.BucketName,
            UserName = input.UserName!,
            CorrelationId = input.CorrelationId!
        };

    internal static string GetEgressDestinationPath(string destinationPath, string? sourcePath, string? sourceRootFolderPath)
    {
        int? index = sourcePath?.IndexOf(sourceRootFolderPath ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        if (index.HasValue && index.Value == 0 && !string.IsNullOrEmpty(sourceRootFolderPath))
        {
            return destinationPath + sourcePath?.Substring(sourceRootFolderPath.Length).TrimStart('/', '\\');
        }
        else
        {
            return destinationPath + sourcePath;
        }
    }

    private void LogFileConflictTelemetry(int caseId, string sourcePath, string destinationPath, TransferDirection transferDirection, Guid transferId)
    {
        var conflictEvent = new DuplicateFileConflictEvent
        {
            CaseId = caseId,
            SourceFilePath = sourcePath,
            DestinationFilePath = destinationPath,
            ConflictingFileName = Path.GetFileName(sourcePath),
            TransferDirection = transferDirection.ToString(),
            TransferId = transferId.ToString()
        };

        _telemetryClient.TrackEvent(conflictEvent);
    }
}
