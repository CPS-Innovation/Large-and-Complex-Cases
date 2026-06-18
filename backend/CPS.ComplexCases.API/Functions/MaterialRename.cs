using System.Net;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models.Configuration;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Models.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Options;

namespace CPS.ComplexCases.API.Functions;

public class MaterialRename(
  IActivityLogService activityLogService,
  IRequestValidator requestValidator,
  IInitializationHandler initializationHandler,
  ISecurityGroupMetadataService securityGroupMetadataService,
  IOntapHttpClient ontapHttpClient,
  IOptions<FeatureFlagConfig> featureFlags)
{
    private readonly IActivityLogService _activityLogService = activityLogService;
    private readonly IRequestValidator _requestValidator = requestValidator;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;
    private readonly ISecurityGroupMetadataService _securityGroupMetadataService = securityGroupMetadataService;
    private readonly IOntapHttpClient _ontapHttpClient = ontapHttpClient;
    private readonly FeatureFlagConfig _featureFlags = featureFlags.Value;

    [Function(nameof(MaterialRename))]
    [OpenApiOperation(operationId: nameof(MaterialRename), tags: ["Material"], Description = "Rename a material folder for a case.")]
    [CmsAuthValuesAuth]
    [BearerTokenAuth]
    [OpenApiRequestBody(ContentType.ApplicationJson, typeof(MaterialRenameDto), Description = "Body containing the material rename information")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "v1/material/rename")] HttpRequest req, FunctionContext functionContext)
    {
        if (!_featureFlags.MaterialRename)
        {
            return new NotFoundResult();
        }

        var context = functionContext.GetRequestContext();

        var renameRequest = await _requestValidator.GetJsonBody<MaterialRenameDto, MaterialRenameRequestValidator>(req);

        if (!renameRequest.IsValid)
        {
            return new BadRequestObjectResult(renameRequest.ValidationErrors);
        }

        _initializationHandler.Initialize(context.Username, context.CorrelationId, renameRequest.Value.CaseId);

        var securityGroups = await _securityGroupMetadataService.GetUserSecurityGroupsAsync(context.BearerToken);

        var result = await _ontapHttpClient.RenameMaterialAsync(
            context.BearerToken,
            securityGroups.First().VolumeUuid,
            renameRequest.Value.CurrentPath,
            renameRequest.Value.NewPath);

        if (result is not OkResult)
        {
            return result;
        }

        await _activityLogService.CreateActivityLogAsync(
            ActivityLog.Enums.ActionType.MaterialRenamed,
            ActivityLog.Enums.ResourceType.Material,
            renameRequest.Value.CaseId,
            renameRequest.Value.CurrentPath,
            renameRequest.Value.NewPath,
            context.Username);

        return new OkObjectResult("Material renamed successfully.");
    }
}