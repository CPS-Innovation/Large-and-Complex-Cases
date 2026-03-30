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
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.Common.Constants;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Services;

using ContentType = CPS.ComplexCases.API.Constants.ContentType;
using ApiResponseDescriptions = CPS.ComplexCases.API.Constants.ApiResponseDescriptions;

namespace CPS.ComplexCases.API.Functions;

public class DisconnectNetAppConnection(ILogger<DisconnectNetAppConnection> logger,
    ICaseMetadataService caseMetadataService,
    IActivityLogService activityLogService,
    IInitializationHandler initializationHandler)
{
    private readonly ILogger<DisconnectNetAppConnection> _logger = logger;
    private readonly ICaseMetadataService _caseMetadataService = caseMetadataService;
    private readonly IActivityLogService _activityLogService = activityLogService;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;

    [Function(nameof(DisconnectNetAppConnection))]
    [OpenApiOperation(operationId: nameof(DisconnectNetAppConnection), tags: ["NetApp"], Description = "Disconnect a NetApp folder from a case.")]
    [CmsAuthValuesAuth]
    [BearerTokenAuth]
    [OpenApiParameter(name: InputParameters.CaseId, In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The case ID to disconnect from.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Conflict, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Conflict)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.NotFound)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "v1/netapp/connections")] HttpRequest req, FunctionContext functionContext)
    {
        var context = functionContext.GetRequestContext();

        var caseIdQuery = req.Query[InputParameters.CaseId];
        if (string.IsNullOrEmpty(caseIdQuery) || !int.TryParse(caseIdQuery, out var caseId))
        {
            return new BadRequestObjectResult("Invalid or missing caseId query parameter.");
        }

        _initializationHandler.Initialize(context.Username, context.CorrelationId, caseId);

        var existingPath = await _caseMetadataService.ClearNetAppFolderPathAsync(caseId);

        if (existingPath == null)
        {
            return new NotFoundObjectResult($"No NetApp connection found for case ID {caseId}.");
        }
        else if (existingPath == CaseMetadataState.TransferIsActive)
        {
            return new ConflictObjectResult($"Cannot disconnect NetApp connection for case ID {caseId} because there is an active transfer.");
        }
        else if (existingPath == CaseMetadataState.NetAppFolderPathIsNull)
        {
            return new BadRequestObjectResult($"Case ID {caseId} does not have an active NetApp connection.");
        }

        await _activityLogService.CreateActivityLogAsync(
            ActivityLog.Enums.ActionType.DisconnectionFromNetApp,
            ActivityLog.Enums.ResourceType.StorageConnection,
            caseId,
            existingPath,
            existingPath,
            context.Username);

        return new OkResult();
    }
}