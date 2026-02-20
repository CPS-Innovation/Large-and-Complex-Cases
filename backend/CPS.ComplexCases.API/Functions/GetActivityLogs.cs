using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using CPS.ComplexCases.ActivityLog.Models.Responses;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Data.Dtos;

namespace CPS.ComplexCases.API.Functions;

public class GetActivityLogs(ILogger<GetActivityLogs> logger, IActivityLogService activityLogService, IInitializationHandler initializationHandler)
{
    private readonly ILogger<GetActivityLogs> _logger = logger;
    private readonly IActivityLogService _activityLogService = activityLogService;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;

    [Function(nameof(GetActivityLogs))]
    [OpenApiOperation(operationId: nameof(GetActivityLogs), tags: ["ActivityLog"], Description = "Lists filtered activity logs.")]
    [CmsAuthValuesAuth]
    [BearerTokenAuth]
    [OpenApiParameter(name: InputParameters.CaseId, In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "The case ID for filtering activity logs.")]
    [OpenApiParameter(name: InputParameters.FromDate, In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The start date for filtering activity logs.")]
    [OpenApiParameter(name: InputParameters.ToDate, In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The end date for filtering activity logs.")]
    [OpenApiParameter(name: InputParameters.UserId, In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The user ID for filtering activity logs.")]
    [OpenApiParameter(name: InputParameters.ActionType, In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The action type for filtering activity logs.")]
    [OpenApiParameter(name: InputParameters.ResourceType, In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The resource type for filtering activity logs.")]
    [OpenApiParameter(name: InputParameters.ResourceId, In = ParameterLocation.Query, Required = false, Type = typeof(string), Description = "The resource ID for filtering activity logs.")]
    [OpenApiParameter(name: InputParameters.Skip, In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "The number of items to skip.")]
    [OpenApiParameter(name: InputParameters.Take, In = ParameterLocation.Query, Required = false, Type = typeof(int), Description = "The number of items to take.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(ActivityLogsResponse), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/activity/logs")] HttpRequest req, FunctionContext functionContext)
    {
        var context = functionContext.GetRequestContext();
        _initializationHandler.Initialize(context.Username, context.CorrelationId);

        var activityLogFilter = new ActivityLogFilterDto
        {
            CaseId = int.TryParse(req.Query[InputParameters.CaseId], out var caseId) ? caseId : null,
            FromDate = DateTime.TryParse(req.Query[InputParameters.FromDate].FirstOrDefault(), out var fromDate) ? fromDate : null,
            ToDate = DateTime.TryParse(req.Query[InputParameters.ToDate].FirstOrDefault(), out var toDate) ? toDate : null,
            Username = req.Query[InputParameters.UserId].FirstOrDefault(),
            ActionType = req.Query[InputParameters.ActionType].FirstOrDefault(),
            ResourceType = req.Query[InputParameters.ResourceType].FirstOrDefault(),
            ResourceId = req.Query[InputParameters.ResourceId].FirstOrDefault(),
            Skip = int.TryParse(req.Query[InputParameters.Skip], out var skip) ? skip : 0,
            Take = int.TryParse(req.Query[InputParameters.Take], out var take) ? take : 100
        };

        var activityLogs = await _activityLogService.GetActivityLogsAsync(activityLogFilter);

        return new OkObjectResult(activityLogs);
    }
}