using CPS.ComplexCases.Common.Models.Requests;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.FileTransfer.API;

public class TransferOrchestrator
{
    [Function(nameof(TransferOrchestrator))]
    public async Task RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        ILogger logger = context.CreateReplaySafeLogger(nameof(TransferOrchestrator));
        logger.LogInformation("TransferOrchestrator started.");

        var transferRequest = context.GetInput<TransferRequest>();

        try
        {
            // 1. call activity to validate source files

            // 2 . calculate transfer size

            // 3. process files in batches

            // 4. process baches in parallel

            // 5. call verify transfer

            // 6. complete transfer 
        }
        catch (Exception ex)
        {
            // handle failure activity
            logger.LogError(ex, "Error in TransferOrchestrator: {Message}", ex.Message);
            throw;
        }
    }
}