using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
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

        // 2. Initial run: LISTING_FILES

        // 3. List files and create TransferItems

        // 4. Retry run: get failed/specified items, apply pathModifications/overwritePolicy

        // 5. IN_PROGRESS

        // 6. Fan-out: TransferFileActivity for each item

        // 7. Finalize



    }
}