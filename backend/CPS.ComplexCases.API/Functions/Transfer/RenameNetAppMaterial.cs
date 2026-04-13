using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using CPS.ComplexCases.API.Clients.FileTransfer;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Domain.Request;
using CPS.ComplexCases.API.Extensions;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.Common.Helpers;

namespace CPS.ComplexCases.API.Functions.Transfer;

public class RenameNetAppMaterial(
    ILogger<RenameNetAppMaterial> logger,
    IFileTransferClient transferClient,
    IRequestValidator requestValidator,
    ISecurityGroupMetadataService securityGroupMetadataService)
{
    private readonly ILogger<RenameNetAppMaterial> _logger = logger;
    private readonly IFileTransferClient _transferClient = transferClient;
    private readonly IRequestValidator _requestValidator = requestValidator;
    private readonly ISecurityGroupMetadataService _securityGroupMetadataService = securityGroupMetadataService;

    [Function(nameof(RenameNetAppMaterial))]
    [OpenApiOperation(operationId: nameof(RenameNetAppMaterial), tags: ["NetApp"], Description = "Renames a single NetApp file by copying it to the new key and deleting the original.")]
    [CmsAuthValuesAuth]
    [BearerTokenAuth]
    [OpenApiRequestBody(ContentType.ApplicationJson, typeof(RenameNetAppMaterialRequest), Description = "Body containing caseId, sourcePath, and destinationPath.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = ApiResponseDescriptions.Success)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.NotFound)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Conflict, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Conflict)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/netapp/materials/rename")] HttpRequest req,
        FunctionContext functionContext)
    {
        var context = functionContext.GetRequestContext();

        _logger.LogInformation("RenameNetAppMaterial request received. CorrelationId: {CorrelationId}", context.CorrelationId);

        var validatedRequest = await _requestValidator.GetJsonBody<RenameNetAppMaterialRequest, RenameNetAppMaterialRequestValidator>(req);
        if (!validatedRequest.IsValid)
        {
            _logger.LogWarning("Validation failed for RenameNetAppMaterial. CorrelationId: {CorrelationId}, Errors: {Errors}",
                context.CorrelationId, validatedRequest.ValidationErrors);
            return new BadRequestObjectResult(validatedRequest.ValidationErrors);
        }

        var securityGroups = await _securityGroupMetadataService.GetUserSecurityGroupsAsync(context.BearerToken);

        var renameRequest = new Common.Models.Requests.RenameNetAppMaterialRequest
        {
            CaseId = validatedRequest.Value.CaseId,
            SourcePath = validatedRequest.Value.SourcePath,
            DestinationPath = validatedRequest.Value.DestinationPath,
            BearerToken = context.BearerToken,
            BucketName = securityGroups.First().BucketName,
            Username = context.Username,
        };

        var response = await _transferClient.RenameNetAppMaterialAsync(renameRequest, context.CorrelationId);

        return await response.ToActionResult();
    }
}
