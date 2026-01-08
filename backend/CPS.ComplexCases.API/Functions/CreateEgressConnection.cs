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
using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Models.Requests;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Factories;

namespace CPS.ComplexCases.API.Functions;

public class CreateEgressConnection(
  ICaseMetadataService caseMetadataService,
  IEgressClient egressClient,
  IEgressArgFactory egressArgFactory,
  ILogger<CreateEgressConnection> logger,
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
  [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
  public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/egress/connections")] HttpRequest req, FunctionContext functionContext)
  {
    var context = functionContext.GetRequestContext();
    _initializationHandler.Initialize(context.Username, context.CorrelationId);

    var egressConnectionRequest = await _requestValidator.GetJsonBody<CreateEgressConnectionDto, CreateEgressConnectionValidator>(req);

    if (!egressConnectionRequest.IsValid)
    {
      return new BadRequestObjectResult(egressConnectionRequest.ValidationErrors);
    }

    var egressArg = _egressArgFactory.CreateGetWorkspacePermissionArg(egressConnectionRequest.Value.EgressWorkspaceId, context.Username);
    var hasEgressPermission = await _egressClient.GetWorkspacePermission(egressArg);

    if (!hasEgressPermission)
    {
      return new UnauthorizedResult();
    }

    await _caseMetadataService.CreateEgressConnectionAsync(egressConnectionRequest.Value);

    await _activityLogService.CreateActivityLogAsync(
      ActivityLog.Enums.ActionType.ConnectionToEgress,
      ActivityLog.Enums.ResourceType.StorageConnection,
      egressConnectionRequest.Value.CaseId,
      egressConnectionRequest.Value.EgressWorkspaceId,
      egressConnectionRequest.Value.EgressWorkspaceId,
      context.Username);

    return new OkResult();
  }
}