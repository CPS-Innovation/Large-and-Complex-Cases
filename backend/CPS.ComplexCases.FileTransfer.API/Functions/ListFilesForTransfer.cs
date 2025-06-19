using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.Common.Extensions;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Common.Models.Domain.Dtos;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.FileTransfer.API.Factories;
using CPS.ComplexCases.FileTransfer.API.Models.Domain;
using CPS.ComplexCases.FileTransfer.API.Validators;

namespace CPS.ComplexCases.FileTransfer.API.Functions;

public class ListFilesForTransfer(ILogger<ListFilesForTransfer> logger, IStorageClientFactory storageClientFactory)
{
    private readonly ILogger<ListFilesForTransfer> _logger = logger;
    private readonly IStorageClientFactory _storageClientFactory = storageClientFactory;

    [Function(nameof(ListFilesForTransfer))]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "transfer/files")] HttpRequest req, FunctionContext context)
    {
        var request = await ValidatorHelper.GetJsonBody<ListFilesForTransferRequest, ListFilesForTransferValidator>(req);

        if (!request.IsValid)
        {
            _logger.LogWarning("Invalid request to get files for transfer: {Errors}", request.ValidationErrors);
            return new BadRequestObjectResult(request.ValidationErrors);
        }

        var sourceClient = _storageClientFactory.GetClientForDirection(request.Value.TransferDirection);

        var selectedEntities = request.Value.SourcePaths.Select(path => new TransferEntityDto
        {
            Path = path.Path,
            FileId = path.FileId,
            IsFolder = path.IsFolder
        }).ToList();

        var filesForTransfer = await sourceClient.ListFilesForTransferAsync(selectedEntities, request.Value.WorkspaceId, request.Value.CaseId);

        var result = new FilesForTransferResult
        {
            CaseId = request.Value?.CaseId ?? 0,
            WorkspaceId = request.Value?.WorkspaceId ?? null,
            TransferDirection = request.Value?.TransferDirection.ToString() ?? string.Empty,
            Files = filesForTransfer,
            IsInvalid = false,
        };

        if (request.Value != null && request.Value.TransferDirection == TransferDirection.EgressToNetApp)
        {
            var destinationPaths = filesForTransfer.Select(x => new DestinationPath
            {
                Path = request.Value.DestinationPath.EnsureTrailingSlash() + x.RelativePath
            }).ToList();
            var validationResult = await new FilePathValidator().ValidateAsync(destinationPaths);
            result.IsInvalid = validationResult.IsValid;
            result.ValidationErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
        }

        return new OkObjectResult(result);
    }
}