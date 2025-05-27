using CPS.ComplexCases.FileTransfer.API.Durable.Activity;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Orchestration;

public class TransferOrchestrator
{
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

        // 1.  initalize transfer entity
        var transferEntity = new TransferEntity
        {
            Id = input.TransferId,
            Status = TransferStatus.Initiated,
            DestinationPath = input.DestinationPath,
            SourcePaths = input.SourcePaths,
            CaseId = input.CaseId,
            TransferType = input.TransferType,
            Direction = input.TransferDirection,
            TotalFiles = input.SourcePaths.Count,
        };

        await context.CallActivityAsync(
            nameof(IntializeTransfer),
            transferEntity);

        // todo: audit record activity

        // 2. Fan-out: TransferFileActivity for each item
        await context.CallActivityAsync(
            nameof(UpdateTransferStatus),
            new UpdateTransferStatusPayload
            {
                TransferId = input.TransferId,
                Status = TransferStatus.InProgress,
            });

        var tasks = new List<Task>();

        foreach (var sourcePath in input.SourcePaths)
        {
            var transferFilePayload = new TransferFilePayload
            {
                SourcePath = sourcePath,
                DestinationPath = transferEntity.DestinationPath,
                TransferId = transferEntity.Id,
                TransferType = transferEntity.TransferType,
                TransferDirection = transferEntity.Direction,
            };

            tasks.Add(context.CallActivityAsync(
                nameof(TransferFile),
                transferFilePayload));
        }
        await Task.WhenAll(tasks);

        // 3. Finalize



    }
}