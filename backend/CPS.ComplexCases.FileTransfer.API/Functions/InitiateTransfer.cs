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
using CPS.ComplexCases.FileTransfer.API.Services;
using CPS.ComplexCases.FileTransfer.API.Durable.Orchestration;

namespace CPS.ComplexCases.FileTransfer.API.Functions;

public class InitiateTransfer
{
    private readonly ILogger<InitiateTransfer> _logger;
    private readonly ITransferService _transferService;

    public InitiateTransfer(ILogger<InitiateTransfer> logger, ITransferService transferService)
    {
        _transferService = transferService;
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

        var transferResponse = await _transferService.InitiateTransferAsync(transferId, transferRequest.Value, currentCorrelationId);

        await orchestrationClient.ScheduleNewOrchestrationInstanceAsync(nameof(TransferOrchestrator), transferRequest.Value, new StartOrchestrationOptions
        {
            InstanceId = transferId.ToString(),
        });

        return new AcceptedResult($"/api/filetransfer/{transferId}/status", transferResponse);
    }
}