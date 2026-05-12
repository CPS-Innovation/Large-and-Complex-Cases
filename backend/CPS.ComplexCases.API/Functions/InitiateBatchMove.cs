using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.API.Clients.FileTransfer;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Models.Requests;

namespace CPS.ComplexCases.API.Functions;

public class InitiateBatchMove(
    ILogger<InitiateBatchMove> logger,
    IFileTransferClient transferClient,
    IRequestValidator requestValidator,
    ISecurityGroupMetadataService securityGroupMetadataService,
    ICaseMetadataService caseMetadataService,
    IInitializationHandler initializationHandler)
    : InitiateBatchOperationBase(logger, transferClient, requestValidator, securityGroupMetadataService, caseMetadataService, initializationHandler)
{
    [Function(nameof(InitiateBatchMove))]
    [OpenApiOperation(operationId: nameof(InitiateBatchMove), tags: ["NetApp"], Description = "Initiates an asynchronous batch move of files and folders within NetApp. Returns a transferId that can be polled for status.")]
    [CmsAuthValuesAuth]
    [BearerTokenAuth]
    [OpenApiRequestBody(ContentType.ApplicationJson, typeof(MoveNetAppBatchDto), Description = "Body containing the case ID, destination prefix, and list of move operations.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Accepted, contentType: ContentType.ApplicationJson, bodyType: typeof(object), Description = "Move batch accepted. Returns transferId and initial status.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.ApplicationJson, typeof(IEnumerable<string>), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Conflict, contentType: ContentType.ApplicationJson, typeof(string), Description = ApiResponseDescriptions.Conflict)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/netapp/move/batch")] HttpRequest req,
        FunctionContext functionContext)
    {
        return await RunAsync<MoveNetAppBatchDto, MoveNetAppBatchOperationDto, MoveNetAppBatchRequestValidator>(
            req, functionContext, nameof(InitiateBatchMove),
            (dto, bearerToken, bucketName, userName, correlationId) =>
                _transferClient.InitiateBatchMoveAsync(new MoveNetAppBatchRequest
                {
                    CaseId = dto.CaseId,
                    DestinationPrefix = dto.DestinationPrefix,
                    Operations = dto.Operations.Select(op => new MoveNetAppBatchOperationRequest
                    {
                        Type = op.Type.ToString(),
                        SourcePath = op.SourcePath,
                    }).ToList(),
                    BearerToken = bearerToken,
                    BucketName = bucketName,
                    UserName = userName,
                }, correlationId));
    }
}
