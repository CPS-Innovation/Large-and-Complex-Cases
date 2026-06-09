using System.Net;
using System.Text.Json;
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

        var caseNameDto = await _caseNamingService.GenerateCaseName(cmsResponse);
        var folderPathName = caseNameDto.CaseName.EnsureTrailingSlash();

        var securityGroups = await _securityGroupMetadataService.GetUserSecurityGroupsAsync(context.BearerToken);

        var provisionNetAppFoldersRequest = new ProvisionNetAppFoldersRequest
        {
            CaseId = caseId,
            Urn = cmsResponse.Urn,
            TemplateName = provisionFoldersRequest.Value.TemplateFolderPath,
            DestinationFolderPath = folderPathName,
            BucketName = securityGroups.First().BucketName,
            BearerToken = context.BearerToken,
            CaseName = caseNameDto.CaseName,
            OperationName = caseNameDto.OperationName,
            UserName = context.Username
        };

        var response = await _transferClient.ProvisionNetAppFoldersAsync(provisionNetAppFoldersRequest, context.CorrelationId);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to provision NetApp folders for caseId: {CaseId}. StatusCode: {StatusCode}, Response: {Response}",
                caseId, response.StatusCode, await response.Content.ReadAsStringAsync());

            return response.StatusCode switch
            {
                HttpStatusCode.Unauthorized => new UnauthorizedObjectResult("Unauthorized to provision NetApp folders."),
                HttpStatusCode.BadRequest => new BadRequestObjectResult("Invalid request to provision NetApp folders."),
                HttpStatusCode.Conflict => new ConflictObjectResult(await response.Content.ReadAsStringAsync()),
                HttpStatusCode.Forbidden => new ForbidResult("Forbidden from provisioning NetApp folders."),
                HttpStatusCode.NotFound => new NotFoundObjectResult("Template folder not found for provisioning NetApp folders."),
                HttpStatusCode.ServiceUnavailable => new ObjectResult("NetApp service is currently unavailable.") { StatusCode = (int)HttpStatusCode.ServiceUnavailable },
                _ => new ObjectResult("An error occurred while provisioning NetApp folders.") { StatusCode = (int)HttpStatusCode.InternalServerError }
            };
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        var parsedResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);
        var transferId = parsedResponse.GetProperty("id").GetString() ?? string.Empty;

        // Always poll for a terminal status. When File Transfer returns 200 the durable
        // orchestration has completed, but FinalizeTransfer signals the transfer entity
        // asynchronously, so the entity may still show InProgress on the first read.
        var finalStatus = await PollForTerminalStatusAsync(transferId, context.CorrelationId);

        if (finalStatus == null || finalStatus == "Initiated" || finalStatus == "InProgress" || finalStatus == "PartiallyCompleted")
        {
            _logger.LogError("NetApp folder provisioning did not complete for caseId: {CaseId}, TransferId: {TransferId}, Status: {Status}",
                caseId, transferId, finalStatus ?? "timeout");
            return new ObjectResult("NetApp folder provisioning did not complete.") { StatusCode = (int)HttpStatusCode.BadRequest };
        }

        if (finalStatus == null || finalStatus == "Failed")
        {
            _logger.LogError("NetApp folder provisioning failed for caseId: {CaseId}, TransferId: {TransferId}, Status: {Status}",
                caseId, transferId, finalStatus);
            return new ObjectResult("NetApp folder provisioning failed.") { StatusCode = (int)HttpStatusCode.InternalServerError };
        }

        await _caseMetadataService.CreateNetAppConnectionAsync(new Data.Models.Requests.CreateNetAppConnectionDto
        {
            CaseId = caseId,
            NetAppFolderPath = folderPathName,
            OperationName = caseNameDto.OperationName
        });

        try
        {
            await _activityLogService.CreateActivityLogAsync(
                ActivityLog.Enums.ActionType.ConnectionToNetApp,
                ActivityLog.Enums.ResourceType.StorageConnection,
                caseId,
                caseNameDto.CaseName,
                caseNameDto.CaseName,
                context.Username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write activity log for NetApp provisioning for case {CaseId}.", caseId);
        }

        _logger.LogInformation("ProvisionNetAppFolders function completed for caseId: {CaseId}", caseId);

        return new OkObjectResult(folderPathName);
    }

    private async Task<string?> PollForTerminalStatusAsync(string transferId, Guid correlationId)
    {
        var deadline = DateTime.UtcNow.AddSeconds(55);
        while (DateTime.UtcNow < deadline)
        {
            await Task.Delay(TimeSpan.FromSeconds(3));
            var status = await GetTransferStatusAsync(transferId, correlationId);
            if (status is not (null or "Initiated" or "InProgress"))
            {
                return status;
            }
        }
        _logger.LogWarning("Polling timed out for transfer {TransferId}.", transferId);
        return null;
    }

    private async Task<string?> GetTransferStatusAsync(string transferId, Guid correlationId)
    {
        var statusResponse = await _transferClient.GetFileTransferStatusAsync(transferId, correlationId);
        if (!statusResponse.IsSuccessStatusCode)
        {
            _logger.LogWarning("Unexpected HTTP {StatusCode} from GetFileTransferStatus for transfer {TransferId}.",
                statusResponse.StatusCode, transferId);
            return null;
        }
        var body = await statusResponse.Content.ReadAsStringAsync();
        var parsed = JsonSerializer.Deserialize<JsonElement>(body);
        return parsed.GetProperty("status").GetString();
    }
}