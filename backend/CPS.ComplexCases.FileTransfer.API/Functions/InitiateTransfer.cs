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
using CPS.ComplexCases.Common.Services;
using Microsoft.DurableTask.Entities;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;

namespace CPS.ComplexCases.FileTransfer.API.Functions;

public class InitiateTransfer
{
    private readonly ILogger<InitiateTransfer> _logger;
    private readonly ICaseMetadataService _caseMetadataService;

    public InitiateTransfer(ILogger<InitiateTransfer> logger, ICaseMetadataService caseMetadataService)
    {
        _logger = logger;
        _caseMetadataService = caseMetadataService;
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

        var caseMetadata = await _caseMetadataService.GetCaseMetadataForCaseIdAsync(transferRequest.Value.Metadata.CaseId);

        if (caseMetadata?.ActiveTransferId.HasValue == true)
        {
            var entityId = new EntityInstanceId(nameof(TransferEntityState), caseMetadata.ActiveTransferId.Value.ToString());
            var entityState = await orchestrationClient.Entities.GetEntityAsync<TransferEntity>(entityId);

            if (entityState != null && entityState.State != null && entityState.State.Status != TransferStatus.Completed)
            {
                // if the transfer is already in progress, return the current status
                return new AcceptedResult($"/api/filetransfer/{caseMetadata.ActiveTransferId}/status", new TransferResponse
                {
                    Id = caseMetadata.ActiveTransferId.Value,
                    Status = entityState.State.Status,
                    CreatedAt = entityState.State.CreatedAt,
                });
            }
        }

        var currentCorrelationId = req.Headers.GetCorrelationId();
        var transferId = Guid.NewGuid();

        await _caseMetadataService.UpdateActiveTransferIdAsync(
            transferRequest.Value.Metadata.CaseId,
            transferId
        );

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
                WorkspaceId = transferRequest.Value.Metadata.WorkspaceId,
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