using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Domain.Response;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Data.Models.Requests;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;

namespace CPS.ComplexCases.API.Functions;

public class DeleteNetAppBatch(
    ILogger<DeleteNetAppBatch> logger,
    INetAppClient netAppClient,
    INetAppArgFactory netAppArgFactory,
    IActivityLogService activityLogService,
    IRequestValidator requestValidator,
    ISecurityGroupMetadataService securityGroupMetadataService,
    IInitializationHandler initializationHandler)
{
    private readonly ILogger<DeleteNetAppBatch> _logger = logger;
    private readonly INetAppClient _netAppClient = netAppClient;
    private readonly INetAppArgFactory _netAppArgFactory = netAppArgFactory;
    private readonly IActivityLogService _activityLogService = activityLogService;
    private readonly IRequestValidator _requestValidator = requestValidator;
    private readonly ISecurityGroupMetadataService _securityGroupMetadataService = securityGroupMetadataService;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;

    [Function(nameof(DeleteNetAppBatch))]
    [OpenApiOperation(operationId: nameof(DeleteNetAppBatch), tags: ["NetApp"], Description = "Delete one or more files and folders from NetApp in a single batch request.")]
    [CmsAuthValuesAuth]
    [BearerTokenAuth]
    [OpenApiRequestBody(ContentType.ApplicationJson, typeof(DeleteNetAppBatchDto), Description = "Body containing the case ID and list of files/folders to delete.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(DeleteNetAppBatchResponse), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/netapp/delete/batch")] HttpRequest req,
        FunctionContext functionContext)
    {
        var context = functionContext.GetRequestContext();
        _initializationHandler.Initialize(context.Username, context.CorrelationId);

        var batchRequest = await _requestValidator.GetJsonBody<DeleteNetAppBatchDto, DeleteNetAppBatchRequestValidator>(req);

        if (!batchRequest.IsValid)
            return new BadRequestObjectResult(batchRequest.ValidationErrors);

        var securityGroups = await _securityGroupMetadataService.GetUserSecurityGroupsAsync(context.BearerToken);
        var bucket = securityGroups.First().BucketName;

        var results = new List<DeleteNetAppBatchItemResult>();

        foreach (var op in batchRequest.Value.Operations)
        {
            var isFolder = op.Type == NetAppDeleteOperationType.Folder;
            var arg = _netAppArgFactory.CreateDeleteFileOrFolderArg(
                context.BearerToken, bucket, string.Empty, op.SourcePath, isFolder);

            try
            {
                _logger.LogInformation(
                    "Deleting {Type} from NetApp: SourcePath={SourcePath}",
                    op.Type, op.SourcePath);

                var result = await _netAppClient.DeleteFileOrFolderAsync(arg);

                if (!result.Success)
                {
                    results.Add(new DeleteNetAppBatchItemResult
                    {
                        SourcePath = op.SourcePath,
                        Status = "Failed",
                        Error = result.ErrorMessage
                    });
                }
                else
                {
                    results.Add(new DeleteNetAppBatchItemResult
                    {
                        SourcePath = op.SourcePath,
                        Status = "Deleted",
                        KeysDeleted = result.KeysDeleted > 1 ? result.KeysDeleted : null
                    });

                    try
                    {
                        var actionType = isFolder
                            ? ActivityLog.Enums.ActionType.FolderDeleted
                            : ActivityLog.Enums.ActionType.MaterialDeleted;

                        var resourceType = isFolder
                            ? ActivityLog.Enums.ResourceType.NetAppFolder
                            : ActivityLog.Enums.ResourceType.Material;

                        await _activityLogService.CreateActivityLogAsync(
                            actionType,
                            resourceType,
                            batchRequest.Value.CaseId,
                            op.SourcePath,
                            op.SourcePath,
                            context.Username);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to write activity log for deletion of {SourcePath}.", op.SourcePath);
                    }
                }
            }
            catch (Amazon.S3.AmazonS3Exception ex) when ((int)ex.StatusCode == 423)
            {
                _logger.LogWarning("File {SourcePath} is locked via SMB (HTTP 423).", op.SourcePath);
                results.Add(new DeleteNetAppBatchItemResult
                {
                    SourcePath = op.SourcePath,
                    Status = "Failed",
                    Error = "File is open via SMB (HTTP 423). Close the file and retry."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting {SourcePath} from bucket {BucketName}.", op.SourcePath, bucket);
                results.Add(new DeleteNetAppBatchItemResult
                {
                    SourcePath = op.SourcePath,
                    Status = "Failed",
                    Error = ex.Message
                });
            }
        }

        var succeeded = results.Count(r => r.Status == "Deleted");
        var failed = results.Count(r => r.Status == "Failed");

        return new OkObjectResult(new DeleteNetAppBatchResponse
        {
            TotalRequested = batchRequest.Value.Operations.Count,
            Succeeded = succeeded,
            Failed = failed,
            Results = results
        });
    }
}
