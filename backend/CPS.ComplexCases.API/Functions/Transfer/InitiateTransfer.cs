using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using System.Net;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.API.Domain.Request;
using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.API.Clients.FileTransfer;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Extensions;

namespace CPS.ComplexCases.API.Functions.Transfer;

public class InitiateTransfer(ILogger<InitiateTransfer> logger, IFileTransferClient transferClient)
{
    private readonly ILogger<InitiateTransfer> _logger = logger;
    private readonly IFileTransferClient _transferClient = transferClient;

    [Function(nameof(InitiateTransfer))]
    [OpenApiOperation(operationId: nameof(Run), tags: ["File Transfer"], Description = "Initiate a file transfer.")]
    [OpenApiRequestBody(ContentType.ApplicationJson, typeof(InitiateTransferRequest), Description = "Body containing the file transfer request")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "filetransfer/initiate")] HttpRequest req, FunctionContext functionContext)
    {
        var context = functionContext.GetRequestContext();

        var transferRequest = await ValidatorHelper.GetJsonBody<InitiateTransferRequest, InitiateTransferRequestValidator>(req);

        if (!transferRequest.IsValid)
        {
            return new BadRequestObjectResult(transferRequest.ValidationErrors);
        }

        if (!int.TryParse(req.Query[InputParameters.CaseId].FirstOrDefault(), out var parsedCaseId))
        {
            return new BadRequestObjectResult($"Invalid {InputParameters.CaseId} provided.");
        }

        var request = new TransferRequest
        {
            TransferType = transferRequest.Value.TransferType,
            DestinationPath = transferRequest.Value.DestinationPath,
            SourcePaths = transferRequest.Value.SourcePaths,
            Metadata = new TransferMetadata
            {
                UserName = context.Username,
                CaseId = parsedCaseId,
            }
        };

        var response = await _transferClient.InitiateFileTransferAsync(request, context.CorrelationId);

        return await response.ToActionResult();
    }
}