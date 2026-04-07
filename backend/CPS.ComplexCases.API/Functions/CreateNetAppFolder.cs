using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Data.Models.Requests;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;

namespace CPS.ComplexCases.API.Functions;

public class CreateNetAppFolder(ILogger<CreateNetAppFolder> logger,
    INetAppClient netAppClient,
    INetAppArgFactory netAppArgFactory,
    IActivityLogService activityLogService,
    IRequestValidator requestValidator,
    ISecurityGroupMetadataService securityGroupMetadataService,
    IInitializationHandler initializationHandler)
{
    private readonly ILogger<CreateNetAppFolder> _logger = logger;
    private readonly INetAppClient _netAppClient = netAppClient;
    private readonly INetAppArgFactory _netAppArgFactory = netAppArgFactory;
    private readonly IActivityLogService _activityLogService = activityLogService;
    private readonly IRequestValidator _requestValidator = requestValidator;
    private readonly ISecurityGroupMetadataService _securityGroupMetadataService = securityGroupMetadataService;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;

    [Function(nameof(CreateNetAppFolder))]
    [OpenApiOperation(operationId: nameof(CreateNetAppFolder), tags: ["NetApp"], Description = "Create a folder in NetApp at the specified path.")]
    [CmsAuthValuesAuth]
    [BearerTokenAuth]
    [OpenApiRequestBody(ContentType.ApplicationJson, typeof(CreateNetAppFolderDto), Description = "Body containing the path at which to create the folder and the associated case ID.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Conflict, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Conflict)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/folders")] HttpRequest req, FunctionContext functionContext)
    {
        var context = functionContext.GetRequestContext();
        _initializationHandler.Initialize(context.Username, context.CorrelationId);

        var createFolderRequest = await _requestValidator.GetJsonBody<CreateNetAppFolderDto, CreateNetAppFolderRequestValidator>(req);

        if (!createFolderRequest.IsValid)
            return new BadRequestObjectResult(createFolderRequest.ValidationErrors);

        var folderPath = createFolderRequest.Value.Path.TrimEnd('/');
        var caseId = createFolderRequest.Value.CaseId;

        var securityGroups = await _securityGroupMetadataService.GetUserSecurityGroupsAsync(context.BearerToken);
        var bucketName = securityGroups.First().BucketName;

        var folderName = folderPath.Contains('/') ? folderPath[(folderPath.LastIndexOf('/') + 1)..] : folderPath;

        var listArg = _netAppArgFactory.CreateListFoldersInBucketArg(context.BearerToken, bucketName, null, null, null, folderPath);
        var existing = await _netAppClient.ListFoldersInBucketAsync(listArg);

        if (existing?.Data?.FolderData != null &&
            existing.Data.FolderData.Any(f => f.Path?.TrimEnd('/').Equals(folderPath, StringComparison.OrdinalIgnoreCase) == true))
        {
            _logger.LogWarning("Folder already exists at path {FolderPath} in bucket {BucketName}.", folderPath, bucketName);
            return new ConflictObjectResult($"A folder already exists at path '{folderPath}'.");
        }

        _logger.LogInformation("Creating NetApp folder at path {FolderPath} in bucket {BucketName} for case {CaseId}.",
            folderPath, bucketName, caseId);

        var createArg = _netAppArgFactory.CreateCreateFolderArg(context.BearerToken, bucketName, folderPath);
        var result = await _netAppClient.CreateFolderAsync(createArg);

        if (!result)
        {
            _logger.LogError("Failed to create folder at path {FolderPath} in bucket {BucketName}.", folderPath, bucketName);
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        try
        {
            await _activityLogService.CreateActivityLogAsync(
                ActivityLog.Enums.ActionType.FolderCreated,
                ActivityLog.Enums.ResourceType.NetAppFolder,
                caseId,
                folderPath,
                folderName,
                context.Username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write activity log for folder creation at path {FolderPath}.", folderPath);
        }

        return new OkObjectResult(result);
    }
}
