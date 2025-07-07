using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.Common.Models.Domain.Dtos;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Factories;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using CPS.ComplexCases.FileTransfer.API.Durable.Helpers;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

public class DeleteFiles(ITransferEntityReader transferEntityReader, IStorageClientFactory storageClientFactory, ILogger<DeleteFiles> logger)
{
    private readonly ITransferEntityReader _transferEntityReader = transferEntityReader;
    private readonly IStorageClientFactory _storageClientFactory = storageClientFactory;
    private readonly ILogger<DeleteFiles> _logger = logger;

    [Function(nameof(DeleteFiles))]
    public async Task Run([ActivityTrigger] DeleteFilesPayload payload, CancellationToken cancellationToken = default)
    {
        if (payload == null)
        {
            throw new ArgumentNullException(nameof(payload), "DeleteFilesPayload cannot be null.");
        }

        if (payload.TransferDirection == Common.Models.Domain.Enums.TransferDirection.NetAppToEgress)
        {
            _logger.LogError("Invalid transfer direction for DeleteFiles activity: {TransferDirection}", payload.TransferDirection);
            throw new ArgumentException("Invalid transfer direction for DeleteFiles activity.", nameof(payload));
        }

        var entity = await _transferEntityReader.GetTransferEntityAsync(payload.TransferId, cancellationToken) ?? throw new InvalidOperationException($"Transfer entity with ID {payload.TransferId} not found.");

        var filesToDelete = entity.State.SuccessfulItems
            .Where(x => x.Status == TransferItemStatus.Completed)
            .Select(x => new DeletionEntityDto
            {
                Path = x.SourcePath,
                FileId = x.FileId
            })
            .ToList();

        if (filesToDelete.Count == 0)
        {
            _logger.LogInformation("No files to delete for transfer ID {TransferId}.", payload.TransferId);
            return;
        }

        var storageClient = _storageClientFactory.GetSourceClientForDirection(payload.TransferDirection);

        await storageClient.DeleteFilesAsync(filesToDelete, payload.WorkspaceId);
    }
}