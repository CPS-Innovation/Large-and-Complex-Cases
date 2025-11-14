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
using CPS.ComplexCases.API.Extensions;
using CPS.ComplexCases.Common.Attributes;

namespace CPS.ComplexCases.API.Functions.Transfer;

public class GetTransferStatus(ILogger<GetTransferStatus> logger, IFileTransferClient transferClient)
{
    private readonly ILogger<GetTransferStatus> _logger = logger;
    private readonly IFileTransferClient _transferClient = transferClient;

    [Function(nameof(GetTransferStatus))]
    [OpenApiOperation(operationId: nameof(Run), tags: ["FileTransfer"], Description = "Get status of a file transfer.")]
    [FunctionKeyAuth]
    [CmsAuthValuesAuth]
    [BearerTokenAuth]
    [OpenApiParameter(name: "transferId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The Id of the transfer.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.NotFound)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/filetransfer/{transferId}/status")] HttpRequest req, FunctionContext functionContext, string transferId)
    {
        var context = functionContext.GetRequestContext();

        var response = await _transferClient.GetFileTransferStatusAsync(transferId, context.CorrelationId);

        return await response.ToActionResult();
    }
}