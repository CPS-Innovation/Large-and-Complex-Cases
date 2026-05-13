using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Telemetry;
using CPS.ComplexCases.FileTransfer.API.Durable.Activity;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Models.Configuration;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Options;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Orchestration;

public class MoveOrchestrator(
    IOptions<SizeConfig> sizeConfig,
    ITelemetryClient telemetryClient,
    IInitializationHandler initializationHandler)
    : BatchOrchestratorBase<MoveBatchPayload, MoveFileItem>(sizeConfig, telemetryClient, initializationHandler)
{
    protected override TransferType BatchTransferType => TransferType.Move;

    protected override async Task<List<string>> GetSuccessfulSourceKeysAsync(
        TaskOrchestrationContext context,
        MoveBatchPayload input,
        List<TransferResult> allResults)
    {
        var failedDeleteSourceKeySet = await DeleteCopiedSourceFilesAsync(context, input);
        var successfulSourceKeySet = BuildSuccessfulSourceKeySet(allResults, failedDeleteSourceKeySet);
        await DeleteEmptySourceFoldersAsync(context, input, successfulSourceKeySet);
        return successfulSourceKeySet.ToList();
    }

    protected override Task WriteActivityLogAsync(
        TaskOrchestrationContext context,
        MoveBatchPayload input,
        List<string> successfulSourceKeys) =>
        context.CallActivityAsync(
            nameof(WriteMoveActivityLog),
            new WriteMoveActivityLogPayload
            {
                CaseId = input.CaseId,
                UserName = input.UserName,
                CorrelationId = input.CorrelationId,
                OriginalOperations = input.OriginalOperations,
                SuccessfulSourceKeys = successfulSourceKeys,
            });

    [Function(nameof(MoveOrchestrator))]
    public Task RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context) =>
        RunOrchestratorAsync(context, nameof(MoveOrchestrator));

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
            .Select(op => new SourceFolderDeleteSpec
            {
                FolderPath = op.SourcePath,
            })
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
                    SourceFolders = foldersToDelete,
                });
        }
    }
}
