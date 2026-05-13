using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.Common.Extensions;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Models.Requests;
using CPS.ComplexCases.DDEI.Client;
using CPS.ComplexCases.DDEI.Factories;
using CPS.ComplexCases.DDEI.Services;
using CPS.ComplexCases.NetApp.Models.Dto;
using CPS.ComplexCases.API.Clients.FileTransfer;
using CPS.ComplexCases.Common.Models.Requests;

namespace CPS.ComplexCases.Api.Functions;

public class ProvisionNetAppFolders(ILogger<ProvisionNetAppFolders> logger,
    IDdeiClient ddeiClient,
    IDdeiArgFactory ddeiArgFactory,
    ICaseNamingService caseNamingService,
    IFileTransferClient transferClient,
    IRequestValidator requestValidator,
    ISecurityGroupMetadataService securityGroupMetadataService,
    ICaseMetadataService caseMetadataService,
    IActivityLogService activityLogService,
    IInitializationHandler initializationHandler)
{
    private readonly ILogger<ProvisionNetAppFolders> _logger = logger;
    private readonly IDdeiClient _ddeiClient = ddeiClient;
    private readonly IDdeiArgFactory _ddeiArgFactory = ddeiArgFactory;
    private readonly ICaseNamingService _caseNamingService = caseNamingService;
    private readonly IFileTransferClient _transferClient = transferClient;
    private readonly IRequestValidator _requestValidator = requestValidator;
    private readonly ISecurityGroupMetadataService _securityGroupMetadataService = securityGroupMetadataService;
    private readonly ICaseMetadataService _caseMetadataService = caseMetadataService;
    private readonly IActivityLogService _activityLogService = activityLogService;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;

    [Function(nameof(ProvisionNetAppFolders))]
    [OpenApiOperation(operationId: nameof(ProvisionNetAppFolders), tags: ["NetApp"], Description = "Create the NetApp folder structure from a template for a case.")]
    [CmsAuthValuesAuth]
    [BearerTokenAuth]
    [OpenApiRequestBody(ContentType.ApplicationJson, typeof(ProvisionNetAppFoldersDto), Description = "Body containing the NetApp folder template to provision")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/cases/{caseId}/netapp/provision")] HttpRequest req, FunctionContext functionContext, int caseId)
    {
        if (caseId <= 0)
        {
            return new BadRequestObjectResult("Invalid caseId parameter.");
        }

        var context = functionContext.GetRequestContext();
        _initializationHandler.Initialize(context.Username, context.CorrelationId, caseId);

        _logger.LogInformation("ProvisionNetAppFolders function triggered for caseId: {CaseId}", caseId);

        var provisionFoldersRequest = await _requestValidator.GetJsonBody<ProvisionNetAppFoldersDto, ProvisionNetAppFoldersRequestValidator>(req);

        if (!provisionFoldersRequest.IsValid)
        {
            return new BadRequestObjectResult(provisionFoldersRequest.ValidationErrors);
        }

        var cmsArg = _ddeiArgFactory.CreateCaseArg(context.CmsAuthValues, context.CorrelationId, caseId);
        var cmsResponse = await _ddeiClient.GetCaseAsync(cmsArg);

        if (cmsResponse == null)
        {
            return new NotFoundObjectResult($"Case with ID {caseId} not found in CMS.");
        }

        var caseMetadata = await _caseMetadataService.GetCaseMetadataForCaseIdAsync(caseId);

        if (!string.IsNullOrEmpty(caseMetadata?.NetappFolderPath))
        {
            return new ConflictObjectResult($"Case with ID {caseId} already has a NetApp folder provisioned.");
        }
        if (caseMetadata?.ActiveTransferId.HasValue == true)
        {
            return new ConflictObjectResult("Cannot provision NetApp folders while there is an active transfer for the case.");
        }

        var caseName = await _caseNamingService.GenerateCaseName(cmsResponse);
        var folderPathName = caseName.EnsureTrailingSlash();

        var securityGroups = await _securityGroupMetadataService.GetUserSecurityGroupsAsync(context.BearerToken);

        var provisionNetAppFoldersRequest = new ProvisionNetAppFoldersRequest
        {
            CaseId = caseId,
            Urn = cmsResponse.Urn,
            TemplateName = provisionFoldersRequest.Value.TemplateFolderPath,
            DestinationFolderPath = folderPathName,
            BucketName = securityGroups.First().BucketName,
            BearerToken = context.BearerToken,
            UserName = context.Username
        };

        await _transferClient.ProvisionNetAppFoldersAsync(provisionNetAppFoldersRequest, context.CorrelationId);

        await _caseMetadataService.CreateNetAppConnectionAsync(new CreateNetAppConnectionDto
        {
            CaseId = caseId,
            NetAppFolderPath = folderPathName,
            OperationName = cmsResponse.OperationName ?? cmsResponse.LeadDefendantSurname ?? caseName
        });

        await _activityLogService.CreateActivityLogAsync(
            ActivityLog.Enums.ActionType.ConnectionToNetApp,
            ActivityLog.Enums.ResourceType.StorageConnection,
            caseId,
            caseName,
            caseName,
            context.Username);

        _logger.LogInformation("ProvisionNetAppFolders function completed for caseId: {CaseId}", caseId);

        return new OkObjectResult(folderPathName);
    }
}