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
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Models.Requests;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;

namespace CPS.ComplexCases.API.Functions;

public class CreateNetAppConnection(ILogger<CreateNetAppConnection> logger,
    ICaseMetadataService caseMetadataService,
    INetAppClient netAppClient,
    INetAppArgFactory netAppArgFactory,
    IActivityLogService activityLogService,
    IRequestValidator requestValidator,
    ISecurityGroupMetadataService securityGroupMetadataService,
    IInitializationHandler initializationHandler)
{
    private readonly ILogger<CreateNetAppConnection> _logger = logger;
    private readonly ICaseMetadataService _caseMetadataService = caseMetadataService;
    private readonly INetAppClient _netAppClient = netAppClient;
    private readonly INetAppArgFactory _netAppArgFactory = netAppArgFactory;
    private readonly IActivityLogService _activityLogService = activityLogService;
    private readonly IRequestValidator _requestValidator = requestValidator;
    private readonly ISecurityGroupMetadataService _securityGroupMetadataService = securityGroupMetadataService;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;

    [Function(nameof(CreateNetAppConnection))]
    [OpenApiOperation(operationId: nameof(CreateEgressConnection), tags: ["NetApp"], Description = "Connect an NetApp folder to a case.")]
    [CmsAuthValuesAuth]
    [BearerTokenAuth]
    [OpenApiRequestBody(ContentType.ApplicationJson, typeof(CreateNetAppConnectionDto), Description = "Body containing the NetApp connection to create")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/netapp/connections")] HttpRequest req, FunctionContext functionContext)
    {
        var context = functionContext.GetRequestContext();
        _initializationHandler.Initialize(context.Username, context.CorrelationId);

        var netAppConnectionRequest = await _requestValidator.GetJsonBody<CreateNetAppConnectionDto, CreateNetAppConnectionValidator>(req);

        if (!netAppConnectionRequest.IsValid)
        {
            return new BadRequestObjectResult(netAppConnectionRequest.ValidationErrors);
        }

        var securityGroups = await _securityGroupMetadataService.GetUserSecurityGroupsAsync(context.BearerToken);

        var netAppArg = _netAppArgFactory.CreateListFoldersInBucketArg(context.BearerToken, securityGroups.First().BucketName, netAppConnectionRequest.Value.OperationName, null, 1, null);
        var hasNetAppPermission = await _netAppClient.ListFoldersInBucketAsync(netAppArg);

        if (hasNetAppPermission == null)
        {
            return new UnauthorizedResult();
        }

        await _caseMetadataService.CreateNetAppConnectionAsync(netAppConnectionRequest.Value);

        await _activityLogService.CreateActivityLogAsync(
            ActivityLog.Enums.ActionType.ConnectionToNetApp,
            ActivityLog.Enums.ResourceType.StorageConnection,
            netAppConnectionRequest.Value.CaseId,
            netAppConnectionRequest.Value.NetAppFolderPath,
            netAppConnectionRequest.Value.NetAppFolderPath,
            context.Username);

        return new OkResult();
    }
}