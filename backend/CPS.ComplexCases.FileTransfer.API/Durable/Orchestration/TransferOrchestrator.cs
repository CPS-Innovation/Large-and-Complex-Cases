using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.FileTransfer.API.Durable.Activity;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Models.Domain;
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
        var transferId = input.TransferId;

        // 1. get transfer details from db
        var transfer = await context.CallActivityAsync<Transfer>(
            nameof(GetTransferDetails),
            transferId);

        List<Guid> itemIdsToProcess;

        // 2. Initial run: LISTING_FILES
        await context.CallActivityAsync<Transfer>(
            nameof(UpdateTransferStatus),
            new UpdateTransferStatusPayload
            {
                TransferId = transferId,
                Status = TransferStatus.ListingFiles,
            });

        // todo: audit record activity

        itemIdsToProcess = await context.CallActivityAsync<List<Guid>>(
            nameof(ListSourceFiles),
            new ListSourceFilesPayload
            {
                TransferId = transferId,
                SourcePaths = transfer.SourcePaths,
                Direction = transfer.Direction,
            });


        // 3. List files and create TransferItems

        // 4. Retry run: get failed/specified items, apply pathModifications/overwritePolicy

        // 5. IN_PROGRESS

        // 6. Fan-out: TransferFileActivity for each item

        // 7. Finalize



    }
}