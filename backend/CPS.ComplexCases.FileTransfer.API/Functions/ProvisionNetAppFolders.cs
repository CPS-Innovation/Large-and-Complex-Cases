using System.Net;
using CPS.ComplexCases.Common.Constants;
using CPS.ComplexCases.Common.Extensions;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.FileTransfer.API.Durable.Orchestration;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using CPS.ComplexCases.FileTransfer.API.Models.Responses;
using CPS.ComplexCases.FileTransfer.API.Validators;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.FileTransfer.API.Functions;

public class ProvisionNetAppFolders(
    ILogger<ProvisionNetAppFolders> logger,
    IRequestValidator requestValidator,
    IInitializationHandler initializationHandler,
    INetAppClient netAppClient,
    INetAppArgFactory netAppArgFactory
)
{
    private readonly ILogger<ProvisionNetAppFolders> _logger = logger;
    private readonly IRequestValidator _requestValidator = requestValidator;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;
    private readonly INetAppClient _netAppClient = netAppClient;
    private readonly INetAppArgFactory _netAppArgFactory = netAppArgFactory;

    [Function(nameof(ProvisionNetAppFolders))]
    [OpenApiOperation(operationId: nameof(ProvisionNetAppFolders), tags: ["NetApp"], Description = "Provisions NetApp folders for a new case based on a predefined template.")]
    [OpenApiParameter(name: HttpHeaderKeys.CorrelationId, In = Microsoft.OpenApi.Models.ParameterLocation.Header, Required = true, Type = typeof(string), Description = "Correlation identifier for tracking the request.")]
    [OpenApiRequestBody(contentType: ContentType.ApplicationJson, bodyType: typeof(ProvisionNetAppFoldersRequest), Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: ContentType.ApplicationJson, bodyType: typeof(TransferResponse), Description = "Provisioning completed within the timeout window.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.Accepted, contentType: ContentType.ApplicationJson, bodyType: typeof(TransferResponse), Description = "Provisioning scheduled but not yet complete. The caller should poll for completion.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: ContentType.ApplicationJson, bodyType: typeof(IEnumerable<string>), Description = ApiResponseDescriptions.BadRequest)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: ContentType.TextPlain, bodyType: typeof(string), Description = ApiResponseDescriptions.InternalServerError)]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "netapp/provision")] HttpRequest req,
        [DurableClient] DurableTaskClient orchestrationClient)
    {
        var correlationId = req.Headers.GetCorrelationId();
        _logger.LogInformation("ProvisionNetAppFolders request received. CorrelationId: {CorrelationId}", correlationId);

        var validatedRequest = await _requestValidator.GetJsonBody<ProvisionNetAppFoldersRequest, ProvisionNetAppFoldersRequestValidator>(req);
        if (!validatedRequest.IsValid)
        {
            _logger.LogWarning("Invalid ProvisionNetAppFolders request. CorrelationId: {CorrelationId}, Errors: {Errors}",
                correlationId, validatedRequest.ValidationErrors);
            return new BadRequestObjectResult(validatedRequest.ValidationErrors);
        }

        var request = validatedRequest.Value;
        _initializationHandler.Initialize(request.UserName ?? string.Empty, correlationId, request.CaseId);

        var templateObjects = new List<FileTransferInfo>();
        var copyFileItems = new List<CopyFileItem>();

        var files = await ListFilesInFolder(
            request.TemplateName,
            request.BearerToken ?? throw new ArgumentNullException(nameof(request.BearerToken), "Bearer token cannot be null."),
            request.BucketName ?? throw new ArgumentNullException(nameof(request.BucketName), "Bucket name cannot be null."));

        if (files != null && files.Any())
        {
            templateObjects.AddRange(files.Select(file => new FileTransferInfo
            {
                SourcePath = file.SourcePath,
                RelativePath = file.SourcePath.RemovePathPrefix(request.TemplateName)
            }));
        }
        else
        {
            _logger.LogWarning("No files found in the specified template folder. CorrelationId: {CorrelationId}, TemplateName: {TemplateName}",
                correlationId, request.TemplateName);

            return new BadRequestObjectResult($"No files found in the specified template folder: {request.TemplateName}");
        }

        foreach (var obj in templateObjects)
        {
            copyFileItems.Add(new CopyFileItem
            {
                SourceKey = obj.SourcePath,
                DestinationPrefix = request.DestinationFolderPath,
                DestinationFileName = obj.RelativePath!,
            });
        }

        var transferId = Guid.NewGuid();

        try
        {
            await orchestrationClient.ScheduleNewOrchestrationInstanceAsync(
                nameof(CopyOrchestrator),
                new CopyBatchPayload
                {
                    TransferId = transferId,
                    CaseId = request.CaseId,
                    UserName = request.UserName,
                    CorrelationId = correlationId,
                    BearerToken = request.BearerToken,
                    BucketName = request.BucketName,
                    Files = copyFileItems,
                    OriginalOperations = [],
                    ManageMaterialsOperationId = transferId,
                    IncludeEmptyFolders = true
                },
                new StartOrchestrationOptions
                {
                    InstanceId = transferId.ToString(),
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to schedule copy orchestration for TransferId: {TransferId}, CaseId: {CaseId}. CorrelationId: {CorrelationId}.",
                transferId, request.CaseId, correlationId);
            throw;
        }

        _logger.LogInformation("Batch copy scheduled. TransferId: {TransferId}, CaseId: {CaseId}, Files: {FileCount}. CorrelationId: {CorrelationId}",
            transferId, request.CaseId, copyFileItems.Count, correlationId);

        var timeout = TimeSpan.FromSeconds(25);
        using var cts = new CancellationTokenSource(timeout);

        OrchestrationMetadata? metadata = null;
        try
        {
            metadata = await orchestrationClient.WaitForInstanceCompletionAsync(
                transferId.ToString(),
                getInputsAndOutputs: false,
                cancellation: cts.Token);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation(
                "Orchestration still running after timeout. Returning 202. TransferId: {TransferId}, CorrelationId: {CorrelationId}",
                transferId, correlationId);
            return new AcceptedResult($"/api/v1/filetransfer/{transferId}/status", new TransferResponse
            {
                Id = transferId,
                Status = TransferStatus.Initiated,
                CreatedAt = DateTime.UtcNow,
            });
        }

        if (metadata.RuntimeStatus == OrchestrationRuntimeStatus.Failed)
        {
            _logger.LogError(
                "Orchestration failed. TransferId: {TransferId}, CaseId: {CaseId}. CorrelationId: {CorrelationId}",
                transferId, request.CaseId, correlationId);
            throw new InvalidOperationException($"Orchestration {transferId} failed.");
        }

        return new OkObjectResult(new TransferResponse
        {
            Id = transferId,
            Status = TransferStatus.Completed,
            CreatedAt = DateTime.UtcNow,
        });
    }

    public async Task<IEnumerable<FileTransferInfo>?> ListFilesInFolder(string path, string bearerToken, string bucketName)
    {
        var filesForTransfer = new List<FileTransferInfo>();

        string? continuationToken = null;
        do
        {
            var arg = _netAppArgFactory.CreateListObjectsInBucketArg(
                bearerToken,
                bucketName,
                continuationToken,
                1000,
                path,
                true);

            var response = await _netAppClient.ListObjectsInBucketAsync(arg);

            if (response == null)
            {
                return filesForTransfer;
            }

            // Add all files from current level
            if (response.Data.FileData.Any())
            {
                filesForTransfer.AddRange(
                    response.Data.FileData.Select(x => new FileTransferInfo
                    {
                        SourcePath = x.Path
                    }));
            }

            // Recursively process all subdirectories
            if (response.Data.FolderData.Any())
            {
                foreach (var folder in response.Data.FolderData)
                {
                    if (!string.IsNullOrEmpty(folder.Path))
                    {
                        filesForTransfer.Add(new FileTransferInfo
                        {
                            SourcePath = folder.Path
                        });

                        var subFiles = await ListFilesInFolder(folder.Path, bearerToken, bucketName);
                        if (subFiles != null)
                        {
                            filesForTransfer.AddRange(subFiles);
                        }
                    }
                }
            }

            continuationToken = response.Pagination.NextContinuationToken;
        } while (continuationToken != null);

        return filesForTransfer;
    }
}