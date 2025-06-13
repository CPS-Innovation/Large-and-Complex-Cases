using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using CPS.ComplexCases.API.Clients.FileTransfer;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Domain.Request;
using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Extensions;

namespace CPS.ComplexCases.API.Functions;

public class GetFilesForTransfer(IFileTransferClient transferClient)
{
    private readonly IFileTransferClient _transferClient = transferClient;

    [Function(nameof(GetFilesForTransfer))]
    [OpenApiOperation(operationId: nameof(GetFilesForTransfer), tags: ["FileTransfer"], Description = "Gets the complete list of files to be transfered from the source storage.")]
    [OpenApiRequestBody(ContentType.ApplicationJson, typeof(GetFilesForTransferRequest), Description = "Body containing the list of files or folders to be transfered from the source storage")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "filetransfer/files")] HttpRequest req, FunctionContext functionContext)
    {
        var context = functionContext.GetRequestContext();

        var request = await ValidatorHelper.GetJsonBody<GetFilesForTransferRequest, GetFilesForTransferRequestValidator>(req);

        if (!request.IsValid)
        {
            return new BadRequestObjectResult(request.ValidationErrors);
        }

        var listFilesForTransferRequest = new ListFilesForTransferRequest
        {
            CaseId = request.Value.CaseId,
            TransferDirection = request.Value.TransferDirection,
            DestinationPath = request.Value.DestinationPath,
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