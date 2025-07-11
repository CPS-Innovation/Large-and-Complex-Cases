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
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.Egress.Constants;
using CPS.ComplexCases.Egress.Models.Args;
using CPS.ComplexCases.FileTransfer.API.Factories;
using CPS.ComplexCases.FileTransfer.API.Models.Domain;
using CPS.ComplexCases.FileTransfer.API.Validators;

namespace CPS.ComplexCases.FileTransfer.API.Functions;

public class ListFilesForTransfer(ILogger<ListFilesForTransfer> logger, IStorageClientFactory storageClientFactory, IRequestValidator requestValidator, IEgressClient egressClient)
{
    private readonly ILogger<ListFilesForTransfer> _logger = logger;
    private readonly IStorageClientFactory _storageClientFactory = storageClientFactory;
    private readonly IRequestValidator _requestValidator = requestValidator;
    private readonly IEgressClient _egressClient = egressClient;

    [Function(nameof(ListFilesForTransfer))]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "transfer/files")] HttpRequest req, FunctionContext context)
    {
        var request = await _requestValidator.GetJsonBody<ListFilesForTransferRequest, ListFilesForTransferValidator>(req);
        if (!request.IsValid)
        {
            _logger.LogWarning("Invalid request to get files for transfer with CorrelationId {CorrelationId}: {Errors}", request.Value.CorrelationId, request.ValidationErrors);
            return new BadRequestObjectResult(request.ValidationErrors);
        }

        var sourceClient = _storageClientFactory.GetSourceClientForDirection(request.Value.TransferDirection);
        var selectedEntities = request.Value.SourcePaths.Select(path => new TransferEntityDto
        {
            Path = path.Path,
            FileId = path.FileId,
            IsFolder = path.IsFolder
        }).ToList();

        if (request.Value != null && request.Value.TransferDirection == TransferDirection.EgressToNetApp && request.Value.TransferType == TransferType.Move)
        {
            var hasPermission = await _egressClient.GetWorkspacePermission(new GetWorkspacePermissionArg
            {
                WorkspaceId = request.Value.WorkspaceId,
                Email = request.Value.Username,
                Permission = EgressFilePermissions.EditDelete
            });

            if (!hasPermission)
            {
                return new UnauthorizedResult();
            }
        }

        var filesForTransfer = await sourceClient.ListFilesForTransferAsync(selectedEntities, request.Value.WorkspaceId, request.Value.CaseId);

        var result = new FilesForTransferResult
        {
            CaseId = request.Value?.CaseId ?? 0,
            WorkspaceId = request.Value?.WorkspaceId ?? null,
            TransferDirection = request.Value?.TransferDirection.ToString() ?? string.Empty,
            Files = filesForTransfer,
            IsInvalid = false,
            DestinationPath = request.Value?.DestinationPath ?? string.Empty,
            ValidationErrors = new List<FileTransferFailedInfo>()
        };

        if (request.Value != null && request.Value.TransferDirection == TransferDirection.EgressToNetApp)
        {
            var destinationPath = request.Value.DestinationPath.EnsureTrailingSlash();
            var validFiles = new List<FileTransferInfo>();
            var failedFiles = new List<FileTransferFailedInfo>();

            var pathValidator = new FilePathValidator();

            foreach (var file in filesForTransfer)
            {
                var fullDestinationPath = destinationPath + file.RelativePath;
                var destinationPaths = new List<DestinationPath> { new DestinationPath { Path = fullDestinationPath } };
                var validationResult = await pathValidator.ValidateAsync(destinationPaths);

                if (validationResult.IsValid)
                {
                    validFiles.Add(file);
                }
                else
                {
                    failedFiles.Add(new FileTransferFailedInfo
                    {
                        Id = file.Id,
                        SourcePath = file.SourcePath,
                        RelativePath = file.RelativePath,
                        Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)),
                        ErrorType = TransferFailedType.PathLengthExceeded
                    });
                }
            }

            result.Files = validFiles;
            result.ValidationErrors = failedFiles;
            result.IsInvalid = failedFiles.Any();
        }

        return new OkObjectResult(result);
    }
}