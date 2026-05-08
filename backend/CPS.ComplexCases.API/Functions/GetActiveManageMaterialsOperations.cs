using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Domain.Response;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Services;

namespace CPS.ComplexCases.API.Functions;

public class GetActiveManageMaterialsOperations(
    ILogger<GetActiveManageMaterialsOperations> logger,
    ICaseActiveManageMaterialsService caseActiveManageMaterialsService,
    IInitializationHandler initializationHandler)
{
    private readonly ILogger<GetActiveManageMaterialsOperations> _logger = logger;
    private readonly ICaseActiveManageMaterialsService _caseActiveManageMaterialsService = caseActiveManageMaterialsService;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;

    [Function(nameof(GetActiveManageMaterialsOperations))]
    [OpenApiOperation(operationId: nameof(GetActiveManageMaterialsOperations), tags: ["NetApp"], Description = "Returns all active manage materials operations for a case. Used by the UI to poll for in-progress MM operations and refresh file tree lock state.")]
    [CmsAuthValuesAuth]
    [BearerTokenAuth]
    [OpenApiParameter(name: "caseId", In = ParameterLocation.Path, Required = true, Type = typeof(int), Description = "The CMS case ID.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(IEnumerable<ActiveManageMaterialsOperationResponse>), Description = "Active MM operations for the case. Empty array when none are in progress.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/cases/{caseId}/manage-materials/active")] HttpRequest req,
        FunctionContext functionContext,
        int caseId)
    {
        var context = functionContext.GetRequestContext();
        _initializationHandler.Initialize(context.Username, context.CorrelationId, caseId);

        _logger.LogInformation("GetActiveManageMaterialsOperations request for CaseId: {CaseId}. CorrelationId: {CorrelationId}",
            caseId, context.CorrelationId);

        var operations = await _caseActiveManageMaterialsService.GetActiveOperationsForCaseAsync(caseId);

        var response = operations.Select(op => new ActiveManageMaterialsOperationResponse
        {
            Id = op.Id,
            OperationType = op.OperationType,
            SourcePaths = op.SourcePaths,
            DestinationPaths = op.DestinationPaths,
            UserName = op.UserName,
            CreatedAt = op.CreatedAt,
        });

        return new OkObjectResult(response);
    }
}
