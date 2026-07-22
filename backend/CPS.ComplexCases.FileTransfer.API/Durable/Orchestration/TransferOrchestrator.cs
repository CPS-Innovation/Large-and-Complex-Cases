using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.Common.Telemetry;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.FileTransfer.API.Durable.Activity;
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

        try
        {
            var (entityId, transferEntity) = await InitializeTransferEntityAsync(context, input);

            var cleanFiles = await FilterDuplicateDestinationFilesAsync(context, input, entityId);

            await PreCreateEgressDestinationFoldersAsync(context, input, cleanFiles, logger);

            var allResults = await FanOutTransferFilesAsync(
                context, input, transferEntity, cleanFiles, entityId, transferOrchestrationEvent);

            await RetryTransientFailuresAsync(
                context, input, transferEntity, cleanFiles, entityId, allResults, transferOrchestrationEvent, logger);

            await DeleteSourceFilesIfMoveAsync(context, input);

            await FinalizeAndLogCompletionAsync(context, input);

            transferOrchestrationEvent.IsSuccessful = transferOrchestrationEvent.TotalFilesFailed == 0;
            transferOrchestrationEvent.OrchestrationEndTime = context.CurrentUtcDateTime;
            _telemetryClient.TrackEvent(transferOrchestrationEvent);
        }
        catch (Exception ex)
        {
            await HandleOrchestratorFailureAsync(context, input, logger, ex);
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
        TransferPayload input)
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

        await context.CallActivityAsync(
            nameof(UpdateActivityLog),
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
        TransferOrchestrationEvent transferOrchestrationEvent)
    {
        await context.CallActivityAsync(
            nameof(UpdateTransferStatus),
            new UpdateTransferStatusPayload
            {
                TransferId = input.TransferId,
                Status = TransferStatus.InProgress,
            });

        int batchSize = _sizeConfig.BatchSize;
        var batch = new List<Task<TransferResult>>();
        var allResults = new List<TransferResult>();

        foreach (var sourcePath in cleanFiles)
        {
            batch.Add(context.CallActivityAsync<TransferResult>(
                nameof(TransferFile),
                BuildTransferFilePayload(input, transferEntity, sourcePath)));

            if (batch.Count >= batchSize)
            {
                var batchResults = await Task.WhenAll(batch);
                await ProcessTransferResults(context, entityId, batchResults, transferOrchestrationEvent);
                allResults.AddRange(batchResults);
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            var remainingResults = await Task.WhenAll(batch);
            await ProcessTransferResults(context, entityId, remainingResults, transferOrchestrationEvent);
            allResults.AddRange(remainingResults);
        }

        return allResults;
    }

    private async Task RetryTransientFailuresAsync(
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
        for (int attempt = 0; attempt < maxOrchestratorRetries; attempt++)
        {
            var retryableFailures = allResults
                .Where(r => r != null && !r.IsSuccess && r.FailedItem?.ErrorCode == TransferErrorCode.Transient)
                .ToList();

            if (retryableFailures.Count == 0) break;

            logger.LogWarning(
                "Orchestrator retry attempt {Attempt}/{MaxRetries}: re-attempting {Count} transiently failed files.",
                attempt + 1, maxOrchestratorRetries, retryableFailures.Count);

            await context.CreateTimer(
                context.CurrentUtcDateTime.Add(TimeSpan.FromSeconds(60 * Math.Pow(2, attempt))),
                CancellationToken.None);

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
                await ProcessTransferResults(context, entityId, batchResults, transferOrchestrationEvent, isRetry: true);
                retryResults.AddRange(batchResults);
            }

            allResults.RemoveAll(r => r != null && !r.IsSuccess && r.FailedItem?.ErrorCode == TransferErrorCode.Transient);
            allResults.AddRange(retryResults);
        }
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

    private static async Task FinalizeAndLogCompletionAsync(TaskOrchestrationContext context, TransferPayload input)
    {
        await context.CallActivityAsync(
            nameof(FinalizeTransfer),
            new FinalizeTransferPayload
            {
                TransferId = input.TransferId,
            });

        await context.CallActivityAsync(
            nameof(UpdateActivityLog),
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
        Exception ex)
    {
        logger.LogError(ex, "TransferOrchestrator failed for TransferId: {TransferId}. With CorrelationId {CorrelationId}", input.TransferId, input.CorrelationId);

        await context.CallActivityAsync(
            nameof(UpdateTransferStatus),
            new UpdateTransferStatusPayload
            {
                TransferId = input.TransferId,
                Status = TransferStatus.Failed,
            });

        await context.CallActivityAsync(
            nameof(UpdateActivityLog),
            new UpdateActivityLogPayload
            {
                ActionType = ActivityLog.Enums.ActionType.TransferFailed,
                TransferId = input.TransferId.ToString(),
                UserName = input.UserName,
                CorrelationId = input.CorrelationId,
                ExceptionMessage = ex.Message
            });
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

    /// <summary>
    /// Processes transfer results and updates the entity synchronously
    /// This eliminates the race condition by ensuring all entity updates are completed
    /// before the orchestrator continues
    /// </summary>
    private static async Task ProcessTransferResults(
        TaskOrchestrationContext context,
        EntityInstanceId entityId,
        TransferResult[] results,
        TransferOrchestrationEvent telemetryEvent,
        bool isRetry = false)
    {
        foreach (var result in results)
        {
            if (result != null && result.IsSuccess && result.SuccessfulItem != null)
            {
                await context.Entities.CallEntityAsync(
                    entityId,
                    isRetry ? nameof(TransferEntityState.AddSuccessfulRetryItem)
                            : nameof(TransferEntityState.AddSuccessfulItem),
                    result.SuccessfulItem);

                telemetryEvent.TotalFilesTransferred++;
                telemetryEvent.TotalBytesTransferred += result.SuccessfulItem.Size;
            }
            else if (result != null && !result.IsSuccess && result.FailedItem != null)
            {
                await context.Entities.CallEntityAsync(
                    entityId,
                    isRetry ? nameof(TransferEntityState.AddFailedRetryItem)
                            : nameof(TransferEntityState.AddFailedItem),
                    result.FailedItem);

                telemetryEvent.TotalFilesFailed++;
            }
        }
    }

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
