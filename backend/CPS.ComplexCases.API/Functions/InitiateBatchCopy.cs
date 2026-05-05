using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.API.Clients.FileTransfer;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Extensions;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Models.Requests;

namespace CPS.ComplexCases.API.Functions;

public class InitiateBatchCopy(
    ILogger<InitiateBatchCopy> logger,
    IFileTransferClient transferClient,
    IRequestValidator requestValidator,
    ISecurityGroupMetadataService securityGroupMetadataService,
    ICaseMetadataService caseMetadataService,
    IInitializationHandler initializationHandler)
{
    private readonly ILogger<InitiateBatchCopy> _logger = logger;
    private readonly IFileTransferClient _transferClient = transferClient;
    private readonly IRequestValidator _requestValidator = requestValidator;
    private readonly ISecurityGroupMetadataService _securityGroupMetadataService = securityGroupMetadataService;
    private readonly ICaseMetadataService _caseMetadataService = caseMetadataService;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;

    [Function(nameof(InitiateBatchCopy))]
    [OpenApiOperation(operationId: nameof(InitiateBatchCopy), tags: ["NetApp"], Description = "Initiates an asynchronous batch copy of files and folders within NetApp. Returns a transferId that can be polled for status.")]
    [CmsAuthValuesAuth]
    [BearerTokenAuth]
    [OpenApiRequestBody(ContentType.ApplicationJson, typeof(CopyNetAppBatchDto), Description = "Body containing the case ID, destination prefix, and list of copy operations.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Accepted, contentType: ContentType.ApplicationJson, bodyType: typeof(object), Description = "Copy batch accepted. Returns transferId and initial status.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.ApplicationJson, typeof(IEnumerable<string>), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Conflict, contentType: ContentType.ApplicationJson, typeof(string), Description = ApiResponseDescriptions.Conflict)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/netapp/copy/batch")] HttpRequest req,
        FunctionContext functionContext)
    {
        var context = functionContext.GetRequestContext();
        _initializationHandler.Initialize(context.Username, context.CorrelationId);

        _logger.LogInformation("InitiateBatchCopy request received. CorrelationId: {CorrelationId}", context.CorrelationId);

        var batchRequest = await _requestValidator.GetJsonBody<CopyNetAppBatchDto, CopyNetAppBatchRequestValidator>(req);

        if (!batchRequest.IsValid)
        {
            _logger.LogWarning("Validation failed for InitiateBatchCopy. CorrelationId: {CorrelationId}, Errors: {Errors}",
                context.CorrelationId, batchRequest.ValidationErrors);
            return new BadRequestObjectResult(batchRequest.ValidationErrors);
        }

        var caseMetadata = await _caseMetadataService.GetCaseMetadataForCaseIdAsync(batchRequest.Value.CaseId);

        if (caseMetadata == null || string.IsNullOrEmpty(caseMetadata.NetappFolderPath))
        {
            _logger.LogWarning("Case metadata or NetApp folder path missing for CaseId: {CaseId}. CorrelationId: {CorrelationId}",
                batchRequest.Value.CaseId, context.CorrelationId);
            return new BadRequestObjectResult(new [] { "Case metadata or NetApp folder path is missing." });
        }

        var casePrefix = caseMetadata.NetappFolderPath.EndsWith('/')
            ? caseMetadata.NetappFolderPath
            : caseMetadata.NetappFolderPath + "/";

        var invalidPaths = batchRequest.Value.Operations
            .Where(op => !op.SourcePath.StartsWith(casePrefix, StringComparison.OrdinalIgnoreCase))
            .Select(op => op.SourcePath)
            .ToList();

        if (invalidPaths.Count > 0)
        {
            _logger.LogWarning("Source paths outside case folder: {Paths}. CorrelationId: {CorrelationId}",
                invalidPaths, context.CorrelationId);
            return new BadRequestObjectResult(new [] { $"The following source paths are not within the case's NetApp folder: {string.Join(", ", invalidPaths)}" });
        }

        if (!batchRequest.Value.DestinationPrefix.StartsWith(casePrefix, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Destination prefix '{DestinationPrefix}' is outside the case folder '{CasePrefix}'. CorrelationId: {CorrelationId}",
                batchRequest.Value.DestinationPrefix, casePrefix, context.CorrelationId);
            return new BadRequestObjectResult(new[] { "The destination prefix is not within the case's NetApp folder." });
        }

        var securityGroups = await _securityGroupMetadataService.GetUserSecurityGroupsAsync(context.BearerToken);

        var copyRequest = new CopyNetAppBatchRequest
        {
            CaseId = batchRequest.Value.CaseId,
            DestinationPrefix = batchRequest.Value.DestinationPrefix,
            Operations = batchRequest.Value.Operations.Select(op => new CopyNetAppBatchOperationRequest
            {
                Type = op.Type == NetAppCopyOperationType.Folder ? "Folder" : "Material",
                SourcePath = op.SourcePath,
            }).ToList(),
            BearerToken = context.BearerToken,
            BucketName = securityGroups.First().BucketName,
            UserName = context.Username,
        };

        var response = await _transferClient.InitiateBatchCopyAsync(copyRequest, context.CorrelationId);

        return await response.ToActionResult();
    }
}
