using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Amazon.S3;
using CPS.ComplexCases.ActivityLog.Enums;
using CPS.ComplexCases.ActivityLog.Extensions;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.Common.Constants;
using CPS.ComplexCases.Common.Extensions;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.FileTransfer.API.Durable.Activity;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Validators;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;

namespace CPS.ComplexCases.FileTransfer.API.Functions;

public class RenameNetAppMaterial(
    ILogger<RenameNetAppMaterial> logger,
    IRequestValidator requestValidator,
    IInitializationHandler initializationHandler,
    ITransferFile transferFile,
    INetAppClient netAppClient,
    INetAppArgFactory netAppArgFactory,
    IActivityLogService activityLogService)
{
    private readonly ILogger<RenameNetAppMaterial> _logger = logger;
    private readonly IRequestValidator _requestValidator = requestValidator;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;
    private readonly ITransferFile _transferFile = transferFile;
    private readonly INetAppClient _netAppClient = netAppClient;
    private readonly INetAppArgFactory _netAppArgFactory = netAppArgFactory;
    private readonly IActivityLogService _activityLogService = activityLogService;

    [Function(nameof(RenameNetAppMaterial))]
    [OpenApiOperation(operationId: nameof(RenameNetAppMaterial), tags: ["NetApp"], Description = "Renames a single NetApp S3 file by copying it to the new key and deleting the original. Pre-flight checks ensure the source exists and the destination does not.")]
    [OpenApiParameter(name: HttpHeaderKeys.CorrelationId, In = Microsoft.OpenApi.Models.ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Correlation identifier for tracking the request.")]
    [OpenApiRequestBody(contentType: ContentType.ApplicationJson, bodyType: typeof(RenameNetAppMaterialRequest), Required = true, Description = "Request containing caseId, sourcePath, destinationPath, and NetApp credentials.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = "File successfully renamed.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.ApplicationJson, bodyType: typeof(IEnumerable<string>), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.NotFound, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = "Source file does not exist.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Conflict, contentType: ContentType.ApplicationJson, bodyType: typeof(string), Description = "Destination already exists, or file is locked via SMB.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, bodyType: typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "netapp/materials/rename")] HttpRequest req)
    {
        var correlationId = req.Headers.GetCorrelationId();
        _logger.LogInformation("Rename NetApp material request received. CorrelationId: {CorrelationId}", correlationId);

        var validatedRequest = await _requestValidator.GetJsonBody<RenameNetAppMaterialRequest, RenameNetAppMaterialValidator>(req);
        if (!validatedRequest.IsValid)
        {
            _logger.LogWarning("Invalid rename request. CorrelationId: {CorrelationId}, Errors: {Errors}", correlationId, validatedRequest.ValidationErrors);
            return new BadRequestObjectResult(validatedRequest.ValidationErrors);
        }

        var request = validatedRequest.Value;
        _initializationHandler.Initialize(request.Username ?? string.Empty, correlationId, request.CaseId);

        var sourceArg = _netAppArgFactory.CreateGetObjectArg(request.BearerToken, request.BucketName, request.SourcePath);
        var sourceExists = await _netAppClient.DoesObjectExistAsync(sourceArg);
        if (!sourceExists)
        {
            _logger.LogWarning("Rename source not found: {SourcePath}. CorrelationId: {CorrelationId}", request.SourcePath, correlationId);
            return new NotFoundObjectResult($"Source file not found: {request.SourcePath}");
        }

        var destArg = _netAppArgFactory.CreateGetObjectArg(request.BearerToken, request.BucketName, request.DestinationPath);
        var destExists = await _netAppClient.DoesObjectExistAsync(destArg);
        if (destExists)
        {
            _logger.LogWarning("Rename destination already exists: {DestinationPath}. CorrelationId: {CorrelationId}", request.DestinationPath, correlationId);
            return new ConflictObjectResult($"A file already exists at the destination path: {request.DestinationPath}");
        }

        var destinationDirectory = (Path.GetDirectoryName(request.DestinationPath) ?? string.Empty)
            .Replace("\\", "/")
            .EnsureTrailingSlash();
        var destinationFileName = Path.GetFileName(request.DestinationPath);

        var payload = new TransferFilePayload
        {
            CaseId = request.CaseId,
            TransferId = Guid.NewGuid(),
            TransferType = TransferType.Copy,
            TransferDirection = TransferDirection.NetAppToNetApp,
            SourcePath = new TransferSourcePath
            {
                Path = request.SourcePath,
                ModifiedPath = destinationFileName,
            },
            DestinationPath = destinationDirectory,
            WorkspaceId = string.Empty,
            BearerToken = request.BearerToken,
            BucketName = request.BucketName,
            UserName = request.Username ?? string.Empty,
            CorrelationId = correlationId,
        };

        var transferResult = await _transferFile.Run(payload);

        if (!transferResult.IsSuccess)
        {
            _logger.LogError("Copy failed for rename {Source} -> {Destination}. CorrelationId: {CorrelationId}. Error: {Error}",
                request.SourcePath, request.DestinationPath, correlationId, transferResult.FailedItem?.ErrorMessage);

            await TryCleanupDestinationAsync(request.BearerToken, request.BucketName, request.DestinationPath, correlationId);

            return new ObjectResult("Failed to copy the file to the new path. The source file has not been modified.")
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        try
        {
            var deleteArg = _netAppArgFactory.CreateDeleteFileOrFolderArg(
                request.BearerToken,
                request.BucketName,
                "DeleteObject",
                request.SourcePath);

            await _netAppClient.DeleteFileOrFolderAsync(deleteArg);
        }
        catch (AmazonS3Exception ex) when ((int)ex.StatusCode == 423)
        {
            _logger.LogWarning(ex,
                "Source file is locked via SMB and could not be deleted after rename copy. Source: {SourcePath}, Destination: {DestinationPath}. CorrelationId: {CorrelationId}",
                request.SourcePath, request.DestinationPath, correlationId);

            return new ConflictObjectResult(
                "The file was copied but the original couldn't be removed — close it and try again.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error deleting source file after successful rename copy. Source: {SourcePath}. CorrelationId: {CorrelationId}",
                request.SourcePath, correlationId);

            return new ObjectResult("The file was copied but the original could not be deleted.")
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        await _activityLogService.CreateActivityLogAsync(
            actionType: ActionType.MaterialRenamed,
            resourceType: ResourceType.Material,
            caseId: request.CaseId,
            resourceId: request.SourcePath,
            resourceName: Path.GetFileName(request.DestinationPath),
            userName: request.Username,
            details: new { sourcePath = request.SourcePath, destinationPath = request.DestinationPath }.SerializeToJsonDocument(_logger));

        _logger.LogInformation("Successfully renamed {SourcePath} to {DestinationPath}. CorrelationId: {CorrelationId}",
            request.SourcePath, request.DestinationPath, correlationId);

        return new OkObjectResult($"File successfully renamed from '{request.SourcePath}' to '{request.DestinationPath}'.");
    }

    private async Task TryCleanupDestinationAsync(string bearerToken, string bucketName, string destinationPath, Guid? correlationId)
    {
        try
        {
            var cleanupArg = _netAppArgFactory.CreateDeleteFileOrFolderArg(bearerToken, bucketName, "DeleteObject", destinationPath);
            await _netAppClient.DeleteFileOrFolderAsync(cleanupArg);
            _logger.LogInformation("Cleaned up partial destination {DestinationPath} after failed copy. CorrelationId: {CorrelationId}", destinationPath, correlationId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to clean up partial destination {DestinationPath} after failed copy. CorrelationId: {CorrelationId}", destinationPath, correlationId);
        }
    }
}
