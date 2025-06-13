using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models.Domain.Dtos;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.FileTransfer.API.Factories;
using CPS.ComplexCases.FileTransfer.API.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

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

        var fileForTransfer = await sourceClient.ListFilesForTransferAsync(selectedEntities, request.Value.WorkspaceId);

        return new OkObjectResult(fileForTransfer);
    }
}