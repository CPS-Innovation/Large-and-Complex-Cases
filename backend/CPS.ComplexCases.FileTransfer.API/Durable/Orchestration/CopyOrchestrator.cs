using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Telemetry;
using CPS.ComplexCases.FileTransfer.API.Durable.Activity;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Models.Configuration;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using CPS.ComplexCases.FileTransfer.API.Telemetry;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Orchestration;

public class CopyOrchestrator(
    IOptions<SizeConfig> sizeConfig,
    ITelemetryClient telemetryClient,
    IInitializationHandler initializationHandler)
    : BatchOrchestratorBase(sizeConfig, telemetryClient, initializationHandler)
{
    [Function(nameof(CopyOrchestrator))]
    public async Task RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        ILogger logger = context.CreateReplaySafeLogger(nameof(CopyOrchestrator));
        logger.LogInformation("CopyOrchestrator started.");

        var input = context.GetInput<CopyBatchPayload>();
        if (input == null)
        {
            logger.LogError("CopyOrchestrator input is null.");
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
                input.CorrelationId, input.BearerToken, input.Files, TransferType.Copy);

            TransferFilePayload BuildPayload(CopyFileItem f) => BuildTransferFilePayload(
                f, input.CaseId, input.TransferId, input.BearerToken,
                input.BucketName, input.UserName!, input.CorrelationId, TransferType.Copy);

            var allResults = await FanOutTransferFilesAsync(context, input.Files, BuildPayload, entityId, telemetryEvent);

            await RetryTransientFailuresAsync(context, entityId, input.Files, allResults, BuildPayload, telemetryEvent, logger, nameof(CopyOrchestrator));

            await context.CallActivityAsync(
                nameof(FinalizeTransfer),
                new FinalizeTransferPayload { TransferId = input.TransferId });

            var successfulSourceKeys = allResults
                .Where(r => r.IsSuccess && r.SuccessfulItem != null)
                .Select(r => r.SuccessfulItem!.SourcePath)
                .ToList();

            await context.CallActivityAsync(
                nameof(WriteCopyActivityLog),
                new WriteCopyActivityLogPayload
                {
                    CaseId = input.CaseId,
                    UserName = input.UserName,
                    CorrelationId = input.CorrelationId,
                    OriginalOperations = input.OriginalOperations,
                    SuccessfulSourceKeys = successfulSourceKeys,
                });

            telemetryEvent.IsSuccessful = telemetryEvent.TotalFilesFailed == 0;
            completedSuccessfully = true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CopyOrchestrator failed for TransferId: {TransferId}. CorrelationId: {CorrelationId}",
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
}
