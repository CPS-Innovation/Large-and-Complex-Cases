using System.Net;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.Common.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace CPS.ComplexCases.API.Functions.Transfer;

public class ClearTransfer(ICaseMetadataService caseMetadataService, ILogger<ClearTransfer> logger)
{
    private readonly ILogger<ClearTransfer> _logger = logger;
    private readonly ICaseMetadataService _caseMetadataService = caseMetadataService;

    [Function(nameof(ClearTransfer))]
    [OpenApiOperation(operationId: nameof(ClearTransfer), tags: ["FileTransfer"], Description = "Clear file transfer notification.")]
    [FunctionKeyAuth]
    [CmsAuthValuesAuth]
    [BearerTokenAuth]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/filetransfer/{transferId}/clear")] HttpRequest req, FunctionContext functionContext, Guid transferId)
    {
        _logger.LogInformation("ClearTransfer function triggered for transferId: {TransferId}", transferId);

        await _caseMetadataService.ClearActiveTransferIdAsync(transferId);

        return new OkResult();
    }
}