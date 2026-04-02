using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.API.Clients.FileTransfer;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Extensions;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Common.Attributes;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.Data.Models.Requests;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;

namespace CPS.ComplexCases.API.Functions;

public class RenameNetAppMaterial(
    ILogger<RenameNetAppMaterial> logger,
    INetAppClient netAppClient,
    INetAppArgFactory netAppArgFactory,
    IFileTransferClient fileTransferClient,
    IRequestValidator requestValidator,
    ISecurityGroupMetadataService securityGroupMetadataService,
    IInitializationHandler initializationHandler)
{
    private readonly ILogger<RenameNetAppMaterial> _logger = logger;
    private readonly INetAppClient _netAppClient = netAppClient;
    private readonly INetAppArgFactory _netAppArgFactory = netAppArgFactory;
    private readonly IFileTransferClient _fileTransferClient = fileTransferClient;
    private readonly IRequestValidator _requestValidator = requestValidator;
    private readonly ISecurityGroupMetadataService _securityGroupMetadataService = securityGroupMetadataService;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;

    [Function(nameof(RenameNetAppMaterial))]
    [OpenApiOperation(operationId: nameof(RenameNetAppMaterial), tags: ["NetApp"], Description = "Rename a material (file) in NetApp. Delegates to the transfer pipeline for large-file support. Returns a transfer ID to poll for completion.")]
    [CmsAuthValuesAuth]
    [BearerTokenAuth]
    [OpenApiRequestBody(ContentType.ApplicationJson, typeof(RenameNetAppMaterialDto), Description = "Body containing the case ID, source path, and new filename")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Accepted, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = "Rename initiated. Response body contains the transfer ID for status polling.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Unauthorized, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Unauthorized)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Forbidden, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.Forbidden)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.NotFound)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Conflict, contentType: ContentType.TextPlain, typeof(string), Description = "A file with the new name already exists in the same location.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "v1/cases/{operationName}/netapp/material")] HttpRequest req,
        string operationName,
        FunctionContext functionContext)
    {
        var context = functionContext.GetRequestContext();

        var renameRequest = await _requestValidator.GetJsonBody<RenameNetAppMaterialDto, RenameNetAppMaterialRequestValidator>(req);

        if (!renameRequest.IsValid)
            return new BadRequestObjectResult(renameRequest.ValidationErrors);

        if (string.IsNullOrWhiteSpace(operationName))
            return new BadRequestObjectResult("Operation name cannot be empty.");

        if (operationName.Contains("..") || operationName.StartsWith('/'))
            return new BadRequestObjectResult("Invalid operation name.");

        _initializationHandler.Initialize(context.Username, context.CorrelationId, renameRequest.Value.CaseId);

        var dto = renameRequest.Value;

        var securityGroups = await _securityGroupMetadataService.GetUserSecurityGroupsAsync(context.BearerToken);
        var bucketName = securityGroups.First().BucketName;

        // Source key is the full S3 key; destination folder is the same directory with the new filename
        var sourceKey = $"{operationName}/{dto.SourcePath}";

        var lastSlash = dto.SourcePath.LastIndexOf('/');
        var sourceDirectory = lastSlash >= 0 ? dto.SourcePath[..lastSlash] : string.Empty;
        var destinationFolder = string.IsNullOrEmpty(sourceDirectory)
            ? operationName
            : $"{operationName}/{sourceDirectory}";

        // Build the dest key only for the pre-flight conflict check
        var destKey = $"{destinationFolder}/{dto.NewName}";

        var sourceExistsArg = _netAppArgFactory.CreateGetObjectArg(context.BearerToken, bucketName, sourceKey);
        if (!await _netAppClient.DoesObjectExistAsync(sourceExistsArg))
        {
            _logger.LogWarning("Rename failed: source file not found. OperationName={OperationName}, SourceKey={SourceKey}", operationName, sourceKey);
            return new NotFoundObjectResult($"Source file '{dto.SourcePath}' not found.");
        }

        var destExistsArg = _netAppArgFactory.CreateGetObjectArg(context.BearerToken, bucketName, destKey);
        if (await _netAppClient.DoesObjectExistAsync(destExistsArg))
        {
            _logger.LogWarning("Rename failed: destination already exists. OperationName={OperationName}, DestKey={DestKey}", operationName, destKey);
            return new ConflictObjectResult($"A file named '{dto.NewName}' already exists in the same location.");
        }

        _logger.LogInformation(
            "Initiating rename via transfer pipeline. OperationName={OperationName}, SourceKey={SourceKey}, DestKey={DestKey}",
            operationName, sourceKey, destKey);

        // Route through the Durable Functions transfer pipeline:
        //   TransferFile copies source → destination (single PUT ≤5 MB, multipart above)
        //   DeleteFiles deletes the source after successful copy
        //   UpdateActivityLog records MaterialRenamed
        //
        // SourcePath.Path        = full source S3 key (read side)
        // SourcePath.ModifiedPath = new filename only (write side — UploadFileAsync combines
        //                           DestinationPath + ModifiedPath to form the destination S3 key)
        var transferRequest = new TransferRequest
        {
            TransferDirection = TransferDirection.NetAppToNetApp,
            TransferType = TransferType.Move,
            DestinationPath = destinationFolder,
            SourcePaths =
            [
                new TransferSourcePath
                {
                    Path = sourceKey,
                    ModifiedPath = dto.NewName
                }
            ],
            Metadata = new TransferMetadata
            {
                CaseId = dto.CaseId,
                UserName = context.Username ?? string.Empty,
                BearerToken = context.BearerToken ?? string.Empty,
                BucketName = bucketName,
                WorkspaceId = string.Empty
            }
        };

        var response = await _fileTransferClient.InitiateFileTransferAsync(transferRequest, context.CorrelationId);

        return await response.ToActionResult();
    }
}
