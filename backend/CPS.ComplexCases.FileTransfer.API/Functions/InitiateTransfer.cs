using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Microsoft.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.Common.Constants;
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
    private readonly IRequestValidator _requestValidator;

    public InitiateTransfer(ILogger<InitiateTransfer> logger, ICaseMetadataService caseMetadataService, IRequestValidator requestValidator)
    {
        _logger = logger;
        _caseMetadataService = caseMetadataService;
        _requestValidator = requestValidator;
    }

    [Function(nameof(InitiateTransfer))]
    [OpenApiOperation(operationId: nameof(InitiateTransfer), tags: ["FileTransfer"], Description = "Initiates a new file transfer operation. If a transfer is already in progress for the case, returns the current transfer status.")]
    [OpenApiRequestBody(contentType: ContentType.ApplicationJson, bodyType: typeof(TransferRequest), Required = true, Description = "Request containing transfer details including source paths, destination, and metadata.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Accepted, contentType: ContentType.ApplicationJson, bodyType: typeof(TransferResponse), Description = "Transfer initiated successfully. Returns transfer ID and status.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.ApplicationJson, bodyType: typeof(IEnumerable<string>), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, bodyType: typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run(
    [HttpTrigger(AuthorizationLevel.Function, "post", Route = "transfer")] HttpRequest req,
    [DurableClient] DurableTaskClient orchestrationClient)
    {
        var currentCorrelationId = req.Headers.GetCorrelationId();
        _logger.LogInformation("Initiating file transfer with CorrelationId: {CorrelationId}", currentCorrelationId);
        var transferRequest = await _requestValidator.GetJsonBody<TransferRequest, TransferRequestValidator>(req);

        if (!transferRequest.IsValid)
        {
            _logger.LogWarning("Invalid transfer request: {Errors} with CorrelationId: {CorrelationId}", transferRequest.ValidationErrors, currentCorrelationId);
            return new BadRequestObjectResult(transferRequest.ValidationErrors);
        }

        if (transferRequest.Value.Metadata == null)
        {
            _logger.LogWarning("Transfer request missing Metadata with CorrelationId: {CorrelationId}.", currentCorrelationId);
            return new BadRequestObjectResult("Metadata is required.");
        }

        var caseMetadata = await _caseMetadataService.GetCaseMetadataForCaseIdAsync(transferRequest.Value.Metadata.CaseId);

        if (caseMetadata?.ActiveTransferId.HasValue == true)
        {
            var entityId = new EntityInstanceId(nameof(TransferEntityState), caseMetadata.ActiveTransferId.Value.ToString());
            var entityState = await orchestrationClient.Entities.GetEntityAsync<TransferEntity>(entityId);

            if (entityState != null && entityState.State != null && entityState.State.Status == TransferStatus.InProgress)
            {
                _logger.LogInformation(
                    "Active transfer detected for CaseId: {CaseId}, TransferId: {TransferId}. Returning current status. CorrelationId: {CorrelationId}",
                    transferRequest.Value.Metadata.CaseId,
                    caseMetadata.ActiveTransferId.Value,
                    currentCorrelationId);

                // if the transfer is already in progress, return the current status
                return new AcceptedResult($"/api/filetransfer/{caseMetadata.ActiveTransferId}/status", new TransferResponse
                {
                    Id = caseMetadata.ActiveTransferId.Value,
                    Status = entityState.State.Status,
                    CreatedAt = entityState.State.CreatedAt,
                });
            }
        }

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
                CorrelationId = currentCorrelationId,
                TransferDirection = transferRequest.Value.TransferDirection,
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