using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.Common.Telemetry;
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
            OrchestrationStartTime = DateTime.UtcNow
        };

        try
        {
            // 1. Initialize transfer entity
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

            // Pre-flight: Single Egress listing (NetApp→Egress direction only)
            var cleanFiles = new List<TransferSourcePath>();
            if (input.TransferDirection == TransferDirection.NetAppToEgress)
            {
                var destinationFiles = await context.CallActivityAsync<HashSet<string>>(
                    nameof(ListDestinationFilePaths),
                    new ListDestinationPayload(input.WorkspaceId, input.DestinationPath));

                // Partition files: duplicates vs clean
                foreach (var sourcePath in input.SourcePaths)
                {
                    var destPath = GetEgressDestinationPath(input.DestinationPath, sourcePath.RelativePath, input.SourceRootFolderPath);
                    if (destinationFiles.Contains(destPath))
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
                    else
                    {
                        cleanFiles.Add(sourcePath);
                    }
                }
            }
            else
            {
                cleanFiles = input.SourcePaths;
            }

            // 2. Fan-out: TransferFileActivity for each item, throttled in batches
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
                var transferFilePayload = new TransferFilePayload
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

                // Activity now returns TransferResult instead of void
                batch.Add(context.CallActivityAsync<TransferResult>(
                    nameof(TransferFile),
                    transferFilePayload));

                if (batch.Count >= batchSize)
                {
                    // Process batch results and update entity synchronously
                    var batchResults = await Task.WhenAll(batch);
                    await ProcessTransferResults(context, entityId, batchResults, transferOrchestrationEvent);
                    allResults.AddRange(batchResults);
                    batch.Clear();
                }
            }

            // Process any remaining tasks in the last batch
            if (batch.Count > 0)
            {
                var remainingResults = await Task.WhenAll(batch);
                await ProcessTransferResults(context, entityId, remainingResults, transferOrchestrationEvent);
                allResults.AddRange(remainingResults);
            }

            // 2b. Orchestrator-level retry for transient S3 failures
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

                // Durable timer backoff ÔÇö no compute consumed during wait
                await context.CreateTimer(
                    context.CurrentUtcDateTime.Add(TimeSpan.FromSeconds(30 * (attempt + 1))),
                    CancellationToken.None);

                // Match failed paths back to original TransferSourcePath objects
                var failedPaths = retryableFailures
                    .Select(r => r.FailedItem!.SourcePath)
                    .ToHashSet();

                var retrySourcePaths = cleanFiles
                    .Where(f => failedPaths.Contains(f.Path) || (f.FullFilePath != null && failedPaths.Contains(f.FullFilePath)))
                    .ToList();

                // Remove transient failures from entity state (undo AddFailedItem counts)
                await context.Entities.CallEntityAsync(
                    entityId,
                    nameof(TransferEntityState.RemoveTransientFailures));

                // Correct telemetry counters
                transferOrchestrationEvent.TotalFilesFailed -= retryableFailures.Count;

                // Re-dispatch as fresh TransferFile activities
                var retryBatch = retrySourcePaths.Select(sp =>
                    context.CallActivityAsync<TransferResult>(
                        nameof(TransferFile),
                        new TransferFilePayload
                        {
                            CaseId = input.CaseId,
                            SourcePath = sp,
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
                        })).ToList();

                var retryResults = await Task.WhenAll(retryBatch);
                await ProcessTransferResults(context, entityId, retryResults, transferOrchestrationEvent, isRetry: true);

                // Replace transient failures with retry results for next iteration
                allResults.RemoveAll(r => r != null && !r.IsSuccess && r.FailedItem?.ErrorCode == TransferErrorCode.Transient);
                allResults.AddRange(retryResults);
            }

            // 3. Delete files if transfer direction is EgressToNetApp and transfer type is Move
            if (input.TransferDirection == TransferDirection.EgressToNetApp && input.TransferType == TransferType.Move)
            {
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

            // 4. Finalize transfer
            await context.CallActivityAsync(
                nameof(FinalizeTransfer),
                new FinalizeTransferPayload
                {
                    TransferId = input.TransferId,
                });

            // 5. Update activity log
            await context.CallActivityAsync(
                nameof(UpdateActivityLog),
                new UpdateActivityLogPayload
                {
                    ActionType = ActivityLog.Enums.ActionType.TransferCompleted,
                    TransferId = input.TransferId.ToString(),
                    UserName = input.UserName,
                    CorrelationId = input.CorrelationId
                });

            transferOrchestrationEvent.IsSuccessful = transferOrchestrationEvent.TotalFilesFailed == 0;
            transferOrchestrationEvent.OrchestrationEndTime = DateTime.UtcNow;
            _telemetryClient.TrackEvent(transferOrchestrationEvent);
        }
        catch (Exception ex)
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

            throw;
        }
        finally
        {
            transferOrchestrationEvent.OrchestrationEndTime = DateTime.UtcNow;
            _telemetryClient.TrackEvent(transferOrchestrationEvent);
        }
    }

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
                // Use CallEntityAsync for synchronous entity updates
                await context.Entities.CallEntityAsync(
                    entityId,
                    isRetry ? nameof(TransferEntityState.AddSuccessfulRetryItem)
                            : nameof(TransferEntityState.AddSuccessfulItem),
                    result.SuccessfulItem);

                // Update telemetry event
                telemetryEvent.TotalFilesTransferred++;
                telemetryEvent.TotalBytesTransferred += result.SuccessfulItem.Size;
            }
            else if (result != null && !result.IsSuccess && result.FailedItem != null)
            {
                // Use CallEntityAsync for synchronous entity updates
                await context.Entities.CallEntityAsync(
                    entityId,
                    isRetry ? nameof(TransferEntityState.AddFailedRetryItem)
                            : nameof(TransferEntityState.AddFailedItem),
                    result.FailedItem);

                // Update telemetry event
                telemetryEvent.TotalFilesFailed++;
            }
        }
    }

    private static string GetEgressDestinationPath(string destinationPath, string? sourcePath, string? sourceRootFolderPath)
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