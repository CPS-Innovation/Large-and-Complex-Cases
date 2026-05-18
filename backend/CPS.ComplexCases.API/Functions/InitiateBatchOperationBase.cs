using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.API.Clients.FileTransfer;
using CPS.ComplexCases.API.Context;
using CPS.ComplexCases.API.Extensions;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Models.Requests;

namespace CPS.ComplexCases.API.Functions;

public abstract class InitiateBatchOperationBase(
    ILogger logger,
    IFileTransferClient transferClient,
    IRequestValidator requestValidator,
    ISecurityGroupMetadataService securityGroupMetadataService,
    ICaseMetadataService caseMetadataService,
    IInitializationHandler initializationHandler)
{
    private readonly ILogger _logger = logger;
    protected readonly IFileTransferClient _transferClient = transferClient;
    private readonly IRequestValidator _requestValidator = requestValidator;
    private readonly ISecurityGroupMetadataService _securityGroupMetadataService = securityGroupMetadataService;
    private readonly ICaseMetadataService _caseMetadataService = caseMetadataService;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;

    protected async Task<IActionResult> RunAsync<TDto, TOperationDto, TValidator>(
        HttpRequest req,
        FunctionContext functionContext,
        string operationName,
        Func<TDto, string, string, string, Guid, Task<HttpResponseMessage>> executeAsync)
        where TDto : INetAppBatchDto<TOperationDto>
        where TOperationDto : INetAppBatchOperationDto
        where TValidator : AbstractValidator<TDto>, new()
    {
        var context = functionContext.GetRequestContext();
        _initializationHandler.Initialize(context.Username, context.CorrelationId);

        _logger.LogInformation("{OperationName} request received. CorrelationId: {CorrelationId}", operationName, context.CorrelationId);

        var batchRequest = await _requestValidator.GetJsonBody<TDto, TValidator>(req);

        if (!batchRequest.IsValid)
        {
            _logger.LogWarning("Validation failed for {OperationName}. CorrelationId: {CorrelationId}, Errors: {Errors}",
                operationName, context.CorrelationId, batchRequest.ValidationErrors);
            return new BadRequestObjectResult(batchRequest.ValidationErrors);
        }

        var caseMetadata = await _caseMetadataService.GetCaseMetadataForCaseIdAsync(batchRequest.Value.CaseId);

        if (caseMetadata == null || string.IsNullOrEmpty(caseMetadata.NetappFolderPath))
        {
            _logger.LogWarning("Case metadata or NetApp folder path missing for CaseId: {CaseId}. CorrelationId: {CorrelationId}",
                batchRequest.Value.CaseId, context.CorrelationId);
            return new BadRequestObjectResult(new[] { "Case metadata or NetApp folder path is missing." });
        }

        var casePrefix = caseMetadata.NetappFolderPath.EndsWith('/')
            ? caseMetadata.NetappFolderPath
            : caseMetadata.NetappFolderPath + "/";

        var invalidPaths = batchRequest.Value.Operations
            .Where(op => !op.SourcePath.StartsWith(casePrefix, StringComparison.OrdinalIgnoreCase))
            .Select(op => op.SourcePath)
            .ToList();

        if (invalidPaths.Count > 0)
        {
            _logger.LogWarning("Source paths outside case folder: {Paths}. CorrelationId: {CorrelationId}",
                invalidPaths, context.CorrelationId);
            return new BadRequestObjectResult(new[] { $"The following source paths are not within the case's NetApp folder: {string.Join(", ", invalidPaths)}" });
        }

        if (!batchRequest.Value.DestinationPrefix.StartsWith(casePrefix, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Destination prefix '{DestinationPrefix}' is outside the case folder '{CasePrefix}'. CorrelationId: {CorrelationId}",
                batchRequest.Value.DestinationPrefix, casePrefix, context.CorrelationId);
            return new BadRequestObjectResult(new[] { "The destination prefix is not within the case's NetApp folder." });
        }

        var securityGroups = await _securityGroupMetadataService.GetUserSecurityGroupsAsync(context.BearerToken);

        var response = await executeAsync(
            batchRequest.Value,
            context.BearerToken,
            securityGroups.First().BucketName,
            context.Username,
            context.CorrelationId);

        return await response.ToActionResult();
    }
}
