using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Microsoft.DurableTask;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.FileTransfer.API.Validators;
using CPS.ComplexCases.Common.Extensions;
using CPS.ComplexCases.FileTransfer.API.Durable.Orchestration;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Models.Responses;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;

namespace CPS.ComplexCases.FileTransfer.API.Functions;

public class InitiateTransfer
{
    private readonly ILogger<InitiateTransfer> _logger;
    public InitiateTransfer(ILogger<InitiateTransfer> logger)
    {
        _logger = logger;
    }

    [Function(nameof(InitiateTransfer))]
    public async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "transfer")] HttpRequest req,
    [DurableClient] DurableTaskClient orchestrationClient)
    {
        var transferRequest = await ValidatorHelper.GetJsonBody<TransferRequest, TransferRequestValidator>(req);

        if (!transferRequest.IsValid)
        {
            return new BadRequestObjectResult(transferRequest.ValidationErrors);
        }

        var currentCorrelationId = req.Headers.GetCorrelationId();
        var transferId = Guid.NewGuid();


        await orchestrationClient.ScheduleNewOrchestrationInstanceAsync(
            nameof(TransferOrchestrator),
            new TransferPayload
            {
                TransferId = transferId,
                TransferType = transferRequest.Value.TransferType,
                DestinationPath = transferRequest.Value.DestinationPath,
                SourcePaths = transferRequest.Value.SourcePaths,
                CaseId = transferRequest.Value.Metadata.CaseId,
                UserName = transferRequest.Value.Metadata.UserName,
            },
            new StartOrchestrationOptions
            {
                InstanceId = transferId.ToString(),
            }
        );

        return new AcceptedResult($"/api/filetransfer/{transferId}/status", new TransferResponse
        {
            Id = transferId,
            Status = TransferStatus.Initiated,
            CreatedAt = DateTime.UtcNow,
        });
    }
}