using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using CPS.ComplexCases.API.Clients.FileTransfer;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Domain.Request;
using CPS.ComplexCases.API.Extensions;
using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Common.Extensions;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models.Requests;

namespace CPS.ComplexCases.API.Functions.Transfer;

public class GetFilesForTransfer(IFileTransferClient transferClient, ILogger<GetFilesForTransfer> logger, IRequestValidator requestValidator)
{
    private readonly IFileTransferClient _transferClient = transferClient;
    private readonly ILogger<GetFilesForTransfer> _logger = logger;
    private readonly IRequestValidator _requestValidator = requestValidator;

    [Function(nameof(GetFilesForTransfer))]
    [OpenApiOperation(operationId: nameof(GetFilesForTransfer), tags: ["FileTransfer"], Description = "Gets the complete list of files to be transferred from the source storage.")]
    [OpenApiRequestBody(ContentType.ApplicationJson, typeof(GetFilesForTransferRequest), Description = "Body containing the list of files or folders to be transferred from the source storage")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/filetransfer/files")] HttpRequest req, FunctionContext functionContext)
    {
        var context = functionContext.GetRequestContext();

        _logger.LogInformation("Getting files for transfer with CorrelationId: {CorrelationId}", context.CorrelationId);

        var request = await _requestValidator.GetJsonBody<GetFilesForTransferRequest, GetFilesForTransferRequestValidator>(req);

        if (!request.IsValid)
        {
            _logger.LogWarning("Validation failed for GetFilesForTransfer: {ValidationErrors}, CorrelationId: {CorrelationId}", request.ValidationErrors, context.CorrelationId);
            return new BadRequestObjectResult(request.ValidationErrors);
        }

        var listFilesForTransferRequest = new ListFilesForTransferRequest
        {
            CaseId = request.Value.CaseId,
            CorrelationId = context.CorrelationId,
            TransferDirection = request.Value.TransferDirection,
            TransferType = request.Value.TransferType,
            DestinationPath = request.Value.DestinationPath,
            WorkspaceId = request.Value.WorkspaceId,
            Username = context.Username,
            SourceRootFolderPath = request.Value.SourceRootFolderPath,
            SourcePaths = request.Value.SourcePaths.Select(path => new SelectedSourcePath
            {
                Path = path.Path,
                FileId = path.FileId,
                IsFolder = path.IsFolder
            }).ToList()
        };

        var response = await _transferClient.ListFilesForTransferAsync(listFilesForTransferRequest, context.CorrelationId);

        return await response.ToActionResult();
    }
}