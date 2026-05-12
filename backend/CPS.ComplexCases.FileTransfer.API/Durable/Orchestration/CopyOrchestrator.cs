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

public class CopyOrchestrator(
    IOptions<SizeConfig> sizeConfig,
    ITelemetryClient telemetryClient,
    IInitializationHandler initializationHandler)
    : BatchOrchestratorBase<CopyBatchPayload, CopyFileItem>(sizeConfig, telemetryClient, initializationHandler)
{
    protected override TransferType BatchTransferType => TransferType.Copy;

    protected override Task<List<string>> GetSuccessfulSourceKeysAsync(
        TaskOrchestrationContext context,
        CopyBatchPayload input,
        List<TransferResult> allResults) =>
        Task.FromResult(allResults
            .Where(r => r.IsSuccess && r.SuccessfulItem != null)
            .Select(r => r.SuccessfulItem!.SourcePath)
            .ToList());

    protected override Task WriteActivityLogAsync(
        TaskOrchestrationContext context,
        CopyBatchPayload input,
        List<string> successfulSourceKeys) =>
        context.CallActivityAsync(
            nameof(WriteCopyActivityLog),
            new WriteCopyActivityLogPayload
            {
                CaseId = input.CaseId,
                UserName = input.UserName,
                CorrelationId = input.CorrelationId,
                OriginalOperations = input.OriginalOperations,
                SuccessfulSourceKeys = successfulSourceKeys,
            });

    [Function(nameof(CopyOrchestrator))]
    public Task RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context) =>
        RunOrchestratorAsync(context, nameof(CopyOrchestrator));
}
