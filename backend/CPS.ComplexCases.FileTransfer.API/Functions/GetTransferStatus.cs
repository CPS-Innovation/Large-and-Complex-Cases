using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using CPS.ComplexCases.Common.Extensions;
using CPS.ComplexCases.Common.Constants;
using Microsoft.DurableTask.Entities;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;

namespace CPS.ComplexCases.FileTransfer.API.Functions;

public class GetTransferStatus
{
    private readonly ILogger<GetTransferStatus> _logger;

    public GetTransferStatus(ILogger<GetTransferStatus> logger)
    {
        _logger = logger;
    }

    [Function(nameof(GetTransferStatus))]
    [OpenApiOperation(operationId: nameof(GetTransferStatus), tags: ["FileTransfer"], Description = "Retrieves the current status and details of a file transfer operation by transfer ID.")]
    [OpenApiParameter(name: "transferId", In = Microsoft.OpenApi.Models.ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The unique identifier of the transfer to retrieve status for.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(TransferEntity), Description = "Transfer status retrieved successfully.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: ContentType.ApplicationJson, bodyType: typeof(object), Description = ApiResponseDescriptions.NotFound)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, bodyType: typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "transfer/{transferId}/status")] HttpRequest req,
        [DurableClient] DurableTaskClient orchestrationClient,
        string transferId)
    {
        var currentCorrelationId = req.Headers.GetCorrelationId();
        var entityId = new EntityInstanceId(nameof(TransferEntityState), transferId);

        _logger.LogInformation("Getting transfer status for ID: {TransferId}, CorrelationId: {CorrelationId}",
            transferId, currentCorrelationId);

        var entityStateResponse = await orchestrationClient.Entities.GetEntityAsync<TransferEntity>(entityId);

        if (entityStateResponse == null)
        {
            _logger.LogWarning("Transfer entity not found for ID: {TransferId}, CorrelationId: {CorrelationId}",
                transferId, currentCorrelationId);

            return new NotFoundObjectResult(new
            {
                Error = "Transfer not found",
                TransferId = transferId,
                CorrelationId = currentCorrelationId
            });
        }

        var transferState = entityStateResponse.State;

        _logger.LogInformation("Successfully retrieved transfer status for ID: {TransferId}, Status: {Status}, CorrelationId: {CorrelationId}",
            transferId, transferState.Status, currentCorrelationId);

        return new OkObjectResult(transferState);
    }
}