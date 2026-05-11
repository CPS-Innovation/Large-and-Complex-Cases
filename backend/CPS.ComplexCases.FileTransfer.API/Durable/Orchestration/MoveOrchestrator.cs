using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Telemetry;
using CPS.ComplexCases.FileTransfer.API.Durable.Activity;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Models.Configuration;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using CPS.ComplexCases.FileTransfer.API.Telemetry;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Orchestration;

public class MoveOrchestrator(
    IOptions<SizeConfig> sizeConfig,
    ITelemetryClient telemetryClient,
    IInitializationHandler initializationHandler)
    : BatchOrchestratorBase(sizeConfig, telemetryClient, initializationHandler)
{
    [Function(nameof(MoveOrchestrator))]
    public async Task RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        ILogger logger = context.CreateReplaySafeLogger(nameof(MoveOrchestrator));
        logger.LogInformation("MoveOrchestrator started.");

        var input = context.GetInput<MoveBatchPayload>();
        if (input == null)
        {
            logger.LogError("MoveOrchestrator input is null.");
            throw new ArgumentNullException(nameof(input));
        }

        _initializationHandler.Initialize(input.UserName!, input.CorrelationId);

        var telemetryEvent = new TransferOrchestrationEvent
        {
            TransferDirection = TransferDirection.NetAppToNetApp.ToString(),
            TotalFiles = input.Files.Count,
            BucketName = input.BucketName,
            CaseId = input.CaseId,
            OrchestrationStartTime = DateTime.UtcNow
        };

        var completedSuccessfully = false;
        try
        {
            var entityId = await InitializeTransferAsync(
                context, input.TransferId, input.CaseId, input.UserName,
                input.CorrelationId, input.BearerToken, input.Files, TransferType.Move);

            TransferFilePayload BuildPayload(MoveFileItem f) => BuildTransferFilePayload(
                f, input.CaseId, input.TransferId, input.BearerToken,
                input.BucketName, input.UserName!, input.CorrelationId, TransferType.Move);

            var allResults = await FanOutTransferFilesAsync(context, input.Files, BuildPayload, entityId, telemetryEvent);

            await RetryTransientFailuresAsync(context, entityId, input.Files, allResults, BuildPayload, telemetryEvent, logger, nameof(MoveOrchestrator));

            var failedDeleteSourceKeySet = await DeleteCopiedSourceFilesAsync(context, input);

            var successfulSourceKeySet = BuildSuccessfulSourceKeySet(allResults, failedDeleteSourceKeySet);

            await DeleteEmptySourceFoldersAsync(context, input, successfulSourceKeySet);

            await context.CallActivityAsync(
                nameof(FinalizeTransfer),
                new FinalizeTransferPayload { TransferId = input.TransferId });

            await context.CallActivityAsync(
                nameof(WriteMoveActivityLog),
                new WriteMoveActivityLogPayload
                {
                    CaseId = input.CaseId,
                    UserName = input.UserName,
                    CorrelationId = input.CorrelationId,
                    OriginalOperations = input.OriginalOperations,
                    SuccessfulSourceKeys = successfulSourceKeySet.ToList(),
                });

            telemetryEvent.IsSuccessful = telemetryEvent.TotalFilesFailed == 0;
            completedSuccessfully = true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "MoveOrchestrator failed for TransferId: {TransferId}. CorrelationId: {CorrelationId}",
                input.TransferId, input.CorrelationId);

            await context.CallActivityAsync(
                nameof(UpdateTransferStatus),
                new UpdateTransferStatusPayload
                {
                    TransferId = input.TransferId,
                    Status = TransferStatus.Failed,
                });

            throw;
        }
        finally
        {
            await context.CallActivityAsync(
                nameof(RemoveActiveManageMaterialsOperation),
                input.ManageMaterialsOperationId);

            telemetryEvent.IsSuccessful = completedSuccessfully && telemetryEvent.TotalFilesFailed == 0;
            telemetryEvent.OrchestrationEndTime = DateTime.UtcNow;
            _telemetryClient.TrackEvent(telemetryEvent);
        }
    }

    private static async Task<HashSet<string>> DeleteCopiedSourceFilesAsync(TaskOrchestrationContext context, MoveBatchPayload input)
    {
        var deletionErrors = await context.CallActivityAsync<List<DeletionError>>(
            nameof(DeleteNetAppFiles),
            new DeleteNetAppFilesPayload
            {
                TransferId = input.TransferId,
                BearerToken = input.BearerToken,
                BucketName = input.BucketName,
                UserName = input.UserName!,
                CorrelationId = input.CorrelationId,
                CaseId = input.CaseId,
            });

        return new HashSet<string>(
            (deletionErrors ?? []).Select(e => e.FileId),
            StringComparer.OrdinalIgnoreCase);
    }

    private static HashSet<string> BuildSuccessfulSourceKeySet(List<TransferResult> allResults, HashSet<string> failedDeleteSourceKeySet) =>
        new(
            allResults
                .Where(r => r.IsSuccess && r.SuccessfulItem != null
                    && !failedDeleteSourceKeySet.Contains(r.SuccessfulItem.SourcePath))
                .Select(r => r.SuccessfulItem!.SourcePath),
            StringComparer.OrdinalIgnoreCase);

    private static async Task DeleteEmptySourceFoldersAsync(
        TaskOrchestrationContext context,
        MoveBatchPayload input,
        HashSet<string> successfulSourceKeySet)
    {
        var foldersToDelete = input.OriginalOperations
            .Where(op => string.Equals(op.Type, "Folder", StringComparison.OrdinalIgnoreCase)
                && op.ExpectedSourceKeys.Count > 0
                && op.ExpectedSourceKeys.All(k => successfulSourceKeySet.Contains(k)))
            .Select(op => op.SourcePath)
            .ToList();

        if (foldersToDelete.Count > 0)
        {
            await context.CallActivityAsync(
                nameof(DeleteNetAppSourceFolders),
                new DeleteNetAppSourceFoldersPayload
                {
                    TransferId = input.TransferId,
                    BearerToken = input.BearerToken,
                    BucketName = input.BucketName,
                    UserName = input.UserName!,
                    CorrelationId = input.CorrelationId,
                    CaseId = input.CaseId,
                    SourceFolderPaths = foldersToDelete,
                });
        }
    }
}
