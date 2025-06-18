using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using System.Net;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Clients.FileTransfer;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Extensions;
using Microsoft.OpenApi.Models;
using CPS.ComplexCases.API.Exceptions;

namespace CPS.ComplexCases.API.Functions.Transfer;

public class GetTransferStatus(ILogger<GetTransferStatus> logger, IFileTransferClient transferClient)
{
    private readonly ILogger<GetTransferStatus> _logger = logger;
    private readonly IFileTransferClient _transferClient = transferClient;

    [Function(nameof(GetTransferStatus))]
    [OpenApiOperation(operationId: nameof(Run), tags: ["File Transfer"], Description = "Get status of a file transfer.")]
    [OpenApiParameter(name: "transferId", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The Id of the transfer.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.NotFound)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "filetransfer/{transferId}/status")] HttpRequest req, FunctionContext functionContext, string transferId)
    {
        try
        {
            var context = functionContext.GetRequestContext();

            var response = await _transferClient.GetFileTransferStatusAsync(transferId, context.CorrelationId);

            return await response.ToActionResult();
        }
        catch (CpsAuthenticationException)
        {
            return new ContentResult
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Content = "Unauthorized"
            };
        }
    }
}