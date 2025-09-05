using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.API.Clients.FileTransfer;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Domain.Request;
using CPS.ComplexCases.API.Extensions;
using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Common.Extensions;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models.Requests;
using Microsoft.OpenApi.Models;

namespace CPS.ComplexCases.API.Functions.Transfer;

public class InitiateTransfer(ILogger<InitiateTransfer> logger, IFileTransferClient transferClient, IRequestValidator requestValidator)
{
    private readonly ILogger<InitiateTransfer> _logger = logger;
    private readonly IFileTransferClient _transferClient = transferClient;
    private readonly IRequestValidator _requestValidator = requestValidator;

    [Function(nameof(InitiateTransfer))]
    [OpenApiOperation(operationId: nameof(Run), tags: ["FileTransfer"], Description = "Initiate a file transfer.")]
    [OpenApiRequestBody(ContentType.ApplicationJson, typeof(InitiateTransferRequest), Description = "Body containing the file transfer request")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/filetransfer/initiate")] HttpRequest req, FunctionContext functionContext)
    {
        var correlationId = req.Headers.GetCorrelationId();

        _logger.LogInformation("Initiating file transfer with CorrelationId: {CorrelationId}", correlationId);
        var context = functionContext.GetRequestContext();

        var transferRequest = await _requestValidator.GetJsonBody<InitiateTransferRequest, InitiateTransferRequestValidator>(req);

        if (!transferRequest.IsValid)
        {
            _logger.LogWarning("Invalid transfer request: {ValidationErrors}, CorrelationId: {CorrelationId}", transferRequest.ValidationErrors, correlationId);
            return new BadRequestObjectResult(transferRequest.ValidationErrors);
        }

        var request = new TransferRequest
        {
            TransferType = transferRequest.Value.TransferType,
            DestinationPath = transferRequest.Value.DestinationPath,
            SourcePaths = transferRequest.Value.SourcePaths.Select((path) => new TransferSourcePath
            {
                Path = path.Path,
                FileId = path.FileId,
                RelativePath = path.RelativePath,
                FullFilePath = path.FullFilePath,
            }).ToList(),
            Metadata = new TransferMetadata
            {
                UserName = context.Username,
                CaseId = transferRequest.Value.CaseId,
                WorkspaceId = transferRequest.Value.WorkspaceId,
            },
            TransferDirection = transferRequest.Value.TransferDirection,
            SourceRootFolderPath = transferRequest.Value.SourceRootFolderPath
        };

        var response = await _transferClient.InitiateFileTransferAsync(request, context.CorrelationId);

        return await response.ToActionResult();
    }
}