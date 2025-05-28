using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using CPS.ComplexCases.FileTransfer.API.Factories;
using CPS.ComplexCases.FileTransfer.API.Models.Configuration;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

public class TransferFile(IStorageClientFactory storageClientFactory, ILogger<TransferFile> logger, SizeConfig sizeConfig)
{
    private readonly IStorageClientFactory _storageClientFactory = storageClientFactory;
    private readonly ILogger<TransferFile> _logger = logger;
    private readonly SizeConfig _sizeConfig = sizeConfig;
    [Function(nameof(TransferFile))]
    public async Task Run([ActivityTrigger] TransferFilePayload payload, [DurableClient] DurableTaskClient client, CancellationToken cancellationToken = default)
    {
        var (sourceClient, destinationClient) = _storageClientFactory.GetClientsForDirection(payload.TransferDirection);
        var entityId = new EntityInstanceId(nameof(TransferEntityState), payload.TransferId.ToString());

        try
        {
            using var sourceStream = await sourceClient.OpenReadStreamAsync(payload.SourcePath.Path, payload.WorkspaceId, payload.SourcePath.FileId);
            var session = await destinationClient.InitiateUploadAsync(payload.DestinationPath, sourceStream.Length, payload.WorkspaceId);

            long totalSize = sourceStream.Length;
            long position = 0;
            int chunkSize = _sizeConfig.ChunkSizeBytes;
            int chunkNumber = 1;

            using var md5 = System.Security.Cryptography.MD5.Create();

            while (position < totalSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int bytesToRead = (int)Math.Min(chunkSize, totalSize - position);
                byte[] buffer = new byte[bytesToRead];
                int bytesRead = await sourceStream.ReadAsync(buffer, 0, bytesToRead, cancellationToken);

                if (bytesRead <= 0)
                    break;

                long start = position;
                long end = start + bytesRead - 1;
                string contentRange = $"{start}-{end}/{totalSize}";

                md5.TransformBlock(buffer, 0, bytesRead, null, 0);

                await destinationClient.UploadChunkAsync(session, chunkNumber, buffer[..bytesRead], contentRange);

                _logger.LogDebug("Uploaded chunk {ChunkNumber} ({Start}-{End})", chunkNumber, start, end);

                position += bytesRead;
                chunkNumber++;
            }

            md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            string md5Hash = md5.Hash != null ? Convert.ToBase64String(md5.Hash) : string.Empty;

            if (destinationClient is EgressStorageClient)
            {
                await destinationClient.CompleteUploadAsync(session, md5hash: md5Hash);
            }
            else
            {
                await destinationClient.CompleteUploadAsync(session);
            }

            _logger.LogInformation("File transfer completed: {SourcePath} -> {DestinationPath}", payload.SourcePath.Path, payload.DestinationPath);
            await client.Entities.SignalEntityAsync(entityId, nameof(TransferEntityState.AddSuccessfulItem));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transfer failed: {Path}", payload.SourcePath.Path);

            var failedItem = new TransferFailedItem
            {
                SourcePath = payload.SourcePath.Path,
                Status = TransferStatus.Failed,
                ErrorCode = "TRANSFER_ERROR",
                ErrorMessage = ex.Message
            };

            await client.Entities.SignalEntityAsync(entityId, nameof(TransferEntityState.AddFailedItem), failedItem);
        }
    }
}
