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
using CPS.ComplexCases.Common.Enums;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Services;

using ContentType = CPS.ComplexCases.API.Constants.ContentType;
using ApiResponseDescriptions = CPS.ComplexCases.API.Constants.ApiResponseDescriptions;
using CPS.ComplexCases.API.Extensions;

namespace CPS.ComplexCases.API.Functions;

public class DisconnectEgressConnection(ILogger<DisconnectEgressConnection> logger,
    ICaseMetadataService caseMetadataService,
    IActivityLogService activityLogService,
    IInitializationHandler initializationHandler)
{
    private readonly ILogger<DisconnectEgressConnection> _logger = logger;
    private readonly ICaseMetadataService _caseMetadataService = caseMetadataService;
    private readonly IActivityLogService _activityLogService = activityLogService;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;

    [Function(nameof(DisconnectEgressConnection))]
    [OpenApiOperation(operationId: nameof(DisconnectEgressConnection), tags: ["Egress"], Description = "Disconnect a Egress workspace from a case.")]
    [CmsAuthValuesAuth]
    [BearerTokenAuth]
    [OpenApiParameter(name: InputParameters.CaseId, In = ParameterLocation.Query, Required = true, Type = typeof(int), Description = "The case ID to disconnect from.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Conflict, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Conflict)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.NotFound)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "v1/egress/connections")] HttpRequest req, FunctionContext functionContext)
    {
        var context = functionContext.GetRequestContext();

        if (!req.TryGetCaseId(out var caseId, out var caseIdError))
        {
            return caseIdError!;
        }

        _initializationHandler.Initialize(context.Username, context.CorrelationId, caseId);

        var result = await _caseMetadataService.ClearEgressConnectionAsync(caseId);

        if (result.State == CaseMetadataState.NoCaseMetadataFound)
        {
            return new NotFoundObjectResult($"No Egress workspace connection found for case ID {caseId}.");
        }
        else if (result.State == CaseMetadataState.TransferIsActive)
        {
            return new ConflictObjectResult($"Cannot disconnect Egress workspace connection for case ID {caseId} because there is an active transfer.");
        }
        else if (result.State == CaseMetadataState.EgressConnectionIsNull)
        {
            return new BadRequestObjectResult($"Case ID {caseId} does not have an active Egress workspace connection.");
        }

        try
        {
            await _activityLogService.CreateActivityLogAsync(
                ActivityLog.Enums.ActionType.DisconnectionFromEgress,
                ActivityLog.Enums.ResourceType.StorageConnection,
                caseId,
                result.ClearedPath!,
                result.ClearedPath,
                context.Username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write activity log for Egress workspace connection disconnection for case {CaseId}.", caseId);
        }

        return new OkResult();
    }
}