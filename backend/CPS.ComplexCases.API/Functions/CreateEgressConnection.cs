using System.Net;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models.Results;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Models.Requests;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace CPS.ComplexCases.API.Functions;

public class CreateEgressConnection(
  ILogger<CreateEgressConnection> logger,
  ICaseMetadataService caseMetadataService,
  IEgressClient egressClient,
  IEgressArgFactory egressArgFactory,
  IActivityLogService activityLogService,
  IRequestValidator requestValidator,
  IInitializationHandler initializationHandler)
{
    private readonly ILogger<CreateEgressConnection> _logger = logger;
    private readonly IActivityLogService _activityLogService = activityLogService;
    private readonly IRequestValidator _requestValidator = requestValidator;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;
    private readonly ICaseMetadataService _caseMetadataService = caseMetadataService;
    private readonly IEgressClient _egressClient = egressClient;
    private readonly IEgressArgFactory _egressArgFactory = egressArgFactory;

    [Function(nameof(CreateEgressConnection))]
    [OpenApiOperation(operationId: nameof(CreateEgressConnection), tags: ["Egress"], Description = "Connect an Egress workspace to a case.")]
    [CmsAuthValuesAuth]
    [BearerTokenAuth]
    [OpenApiRequestBody(ContentType.ApplicationJson, typeof(CreateEgressConnectionDto), Description = "Body containing the Egress connection to create")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Conflict, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Conflict)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/egress/connections")] HttpRequest req, FunctionContext functionContext)
    {
        var context = functionContext.GetRequestContext();

        var egressConnectionRequest = await _requestValidator.GetJsonBody<CreateEgressConnectionDto, CreateEgressConnectionValidator>(req);

        if (!egressConnectionRequest.IsValid)
        {
            return new BadRequestObjectResult(egressConnectionRequest.ValidationErrors);
        }

        _initializationHandler.Initialize(context.Username, context.CorrelationId, egressConnectionRequest.Value.CaseId);

        var egressArg = _egressArgFactory.CreateGetWorkspacePermissionArg(egressConnectionRequest.Value.EgressWorkspaceId, context.Username);
        var hasEgressPermission = await _egressClient.GetWorkspacePermission(egressArg);

        if (!hasEgressPermission)
        {
            return new UnauthorizedResult();
        }

        var caseId = egressConnectionRequest.Value.CaseId;
        var workspaceId = egressConnectionRequest.Value.EgressWorkspaceId;

        var caseMetadata = await _caseMetadataService.GetCaseMetadataForCaseIdAsync(caseId);
        if (!string.IsNullOrEmpty(caseMetadata?.EgressWorkspaceId))
        {
            _logger.LogWarning(
                "Duplicate Egress connection attempt: case {CaseId} is already connected to workspace {ExistingWorkspaceId}. Requested workspace {RequestedWorkspaceId}.",
                caseId, caseMetadata.EgressWorkspaceId, workspaceId);
            return new ConflictObjectResult($"Case with ID {caseId} already has an Egress workspace connected.");
        }

        var existingConnection = (await _caseMetadataService.GetCaseMetadataForEgressWorkspaceIdsAsync([workspaceId])).FirstOrDefault();
        if (existingConnection != null)
        {
            _logger.LogWarning(
                "Duplicate Egress connection attempt: workspace {WorkspaceId} is already connected to case {ExistingCaseId}. Requested by case {RequestedCaseId}.",
                workspaceId, existingConnection.CaseId, caseId);
            return new ConflictObjectResult($"Egress workspace '{workspaceId}' is already connected to another case.");
        }

        var connectionResult = await _caseMetadataService.CreateEgressConnectionAsync(egressConnectionRequest.Value);
        if (connectionResult == CreateConnectionResult.Conflict)
        {
            return new ConflictObjectResult($"Egress workspace '{workspaceId}' could not be connected to case {caseId} because one of them is already connected.");
        }

        await _activityLogService.CreateActivityLogAsync(
          ActivityLog.Enums.ActionType.ConnectionToEgress,
          ActivityLog.Enums.ResourceType.StorageConnection,
          egressConnectionRequest.Value.CaseId,
          egressConnectionRequest.Value.EgressWorkspaceId,
          egressConnectionRequest.Value.EgressWorkspaceName,
          context.Username);

        return new OkResult();
    }
}
