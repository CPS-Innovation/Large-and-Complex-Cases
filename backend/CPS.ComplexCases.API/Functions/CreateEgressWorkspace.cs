using System.Net;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.ActivityLog.Services;
using Microsoft.OpenApi.Models;
using CPS.ComplexCases.API.Domain.Request;
using CPS.ComplexCases.Data.Models.Requests;
using CPS.ComplexCases.DDEI.Client;
using CPS.ComplexCases.DDEI.Factories;
using CPS.ComplexCases.Common.Attributes;

namespace CPS.ComplexCases.API.Functions;

public class CreateEgressWorkspace(
    ICaseMetadataService caseMetadataService,
    IEgressClient egressClient,
    IEgressArgFactory egressArgFactory,
    IDdeiClient ddeiClient,
    IDdeiArgFactory ddeiArgFactory,
    ILogger<CreateEgressWorkspace> logger,
    IActivityLogService activityLogService,
    IRequestValidator requestValidator)
{
    private readonly ILogger<CreateEgressWorkspace> _logger = logger;
    private readonly IActivityLogService _activityLogService = activityLogService;
    private readonly IRequestValidator _requestValidator = requestValidator;
    private readonly ICaseMetadataService _caseMetadataService = caseMetadataService;
    private readonly IEgressClient _egressClient = egressClient;
    private readonly IEgressArgFactory _egressArgFactory = egressArgFactory;
    private readonly IDdeiClient _ddeiClient = ddeiClient;
    private readonly IDdeiArgFactory _ddeiArgFactory = ddeiArgFactory;

    [Function(nameof(CreateEgressWorkspace))]
    [OpenApiOperation(operationId: nameof(CreateEgressWorkspace), tags: ["Egress"], Description = "Create an egress workspace")]
    [CmsAuthValuesAuth]
    [BearerTokenAuth]
    [OpenApiRequestBody(ContentType.ApplicationJson, typeof(CreateEgressWorkspaceRequest), Description = "Body containing the workspace details to create")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/egress/workspaces")] HttpRequest req, FunctionContext functionContext)
    {
        var context = functionContext.GetRequestContext();

        var request = await _requestValidator.GetJsonBody<CreateEgressWorkspaceRequest, CreateEgressWorkspaceRequestValidator>(req);

        if (!request.IsValid)
        {
            return new BadRequestObjectResult(request.ValidationErrors);
        }

        var cmsArg = _ddeiArgFactory.CreateCaseArg(functionContext.GetRequestContext().CmsAuthValues, functionContext.GetRequestContext().CorrelationId, request.Value.CaseId);
        var cmsResponse = await _ddeiClient.GetCaseAsync(cmsArg);

        string workspaceName;
        if (!string.IsNullOrWhiteSpace(cmsResponse.OperationName))
        {
            workspaceName = $"{cmsResponse.OperationName}-{cmsResponse.Urn}";
        }
        else
        {
            workspaceName = $"{cmsResponse.LeadDefendantSurname}-{cmsResponse.Urn}";
        }

        var arg = _egressArgFactory.CreateEgressWorkspaceArg(
            workspaceName,
            request.Value.Description,
            request.Value.TemplateId);

        var workspace = await _egressClient.CreateWorkspaceAsync(arg);

        var rolesForWorkspace = await _egressClient.ListWorkspaceRolesAsync(workspace.Id);
        var administratorRole = rolesForWorkspace.FirstOrDefault(r => r.RoleName == "Administrator")?.RoleId;

        if (administratorRole == null)
        {
            _logger.LogError("Administrator role not found for workspace {WorkspaceId}", workspace.Id);
            throw new Exception("Administrator role not found for workspace");
        }

        var workspacePermissionArg = _egressArgFactory.CreateGrantWorkspacePermissionArg(workspace.Id, context.Username, administratorRole);
        await _egressClient.GrantWorkspacePermission(workspacePermissionArg);

        await _caseMetadataService.CreateEgressConnectionAsync(new CreateEgressConnectionDto
        {
            CaseId = request.Value.CaseId,
            EgressWorkspaceId = workspace.Id
        });

        await _activityLogService.CreateActivityLogAsync(
          ActivityLog.Enums.ActionType.ConnectionToEgress,
          ActivityLog.Enums.ResourceType.StorageConnection,
          request.Value.CaseId,
          workspace.Id,
          workspace.Id,
          context.Username);

        return new OkObjectResult(workspace);
    }
}