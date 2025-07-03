using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using CPS.ComplexCases.FileTransfer.API.Factories;
using CPS.ComplexCases.FileTransfer.API.Models.Configuration;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using CPS.ComplexCases.Common.Models.Domain.Exceptions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

/// <summary>
/// Durable activity for transferring files between storage endpoints.
/// Handles chunked uploads, computes MD5 hashes for integrity, and manages transfer state.
/// Signals success or failure to the associated transfer entity for orchestration.
/// </summary>
public class TransferFile(IStorageClientFactory storageClientFactory, ILogger<TransferFile> logger, IOptions<SizeConfig> sizeConfig)
{
    private readonly IStorageClientFactory _storageClientFactory = storageClientFactory;
    private readonly ILogger<TransferFile> _logger = logger;
    private readonly SizeConfig _sizeConfig = sizeConfig.Value;

    [Function(nameof(TransferFile))]
    public async Task Run([ActivityTrigger] TransferFilePayload payload, [DurableClient] DurableTaskClient client, CancellationToken cancellationToken = default)
    {
        var (sourceClient, destinationClient) = _storageClientFactory.GetClientsForDirection(payload.TransferDirection);
        var entityId = new EntityInstanceId(nameof(TransferEntityState), payload.TransferId.ToString());

        try
        {
            var sourceFilePath = string.IsNullOrEmpty(payload.SourcePath.ModifiedPath) ? payload.SourcePath.Path : payload.SourcePath.ModifiedPath;

            using var sourceStream = await sourceClient.OpenReadStreamAsync(payload.SourcePath.Path, payload.WorkspaceId, payload.SourcePath.FileId);
            var session = await destinationClient.InitiateUploadAsync(payload.DestinationPath, sourceStream.Length, sourceFilePath, payload.WorkspaceId, payload.SourcePath.RelativePath);

            long totalSize = sourceStream.Length;
            long position = 0;
            int chunkSize = _sizeConfig.ChunkSizeBytes;
            int chunkNumber = 1;
            Dictionary<int, string> uploadedChunks = [];

            using var md5 = System.Security.Cryptography.MD5.Create();
            byte[] buffer = new byte[chunkSize];

            while (position < totalSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int bytesToRead = (int)Math.Min(chunkSize, totalSize - position);
                int bytesRead = await sourceStream.ReadAsync(buffer.AsMemory(0, bytesToRead), cancellationToken);

                if (bytesRead <= 0)
                    break;

                long start = position;
                long end = start + bytesRead - 1;
                string contentRange = $"{start}-{end}/{totalSize}";

                md5.TransformBlock(buffer, 0, bytesRead, null, 0);

                var result = await destinationClient.UploadChunkAsync(session, chunkNumber, buffer[..bytesRead], contentRange);

                if (result.TransferDirection == TransferDirection.EgressToNetApp && result.PartNumber.HasValue && result.ETag != null)
                {
                    uploadedChunks.Add(result.PartNumber.Value, result.ETag);
                }

                _logger.LogDebug("Transfer Id: {TransferId} Uploaded chunk: {ChunkNumber} ({Start}-{End})", payload.TransferId, chunkNumber, start, end);

                position += bytesRead;
                chunkNumber++;
            }

            // Finalizes the MD5 hash computation. Array.Empty<byte>() is used because there is no more data to process, but TransformFinalBlock must be called to complete the hash.
            md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            string md5Hash = md5.Hash != null ? Convert.ToBase64String(md5.Hash) : string.Empty;

            // EgressStorageClient requires MD5 hash, while NetAppStorageClient requires ETags for chunked uploads.
            if (destinationClient is EgressStorageClient)
            {
                await destinationClient.CompleteUploadAsync(session, md5hash: md5Hash);
            }
            else
            {
                await destinationClient.CompleteUploadAsync(session, etags: uploadedChunks);
            }

            _logger.LogInformation("File transfer completed: {SourcePath} -> {DestinationPath}", payload.SourcePath.Path, payload.DestinationPath);

            var successfulItem = new TransferItem
            {
                SourcePath = payload.SourcePath.Path,
                Status = TransferItemStatus.Completed,
                Size = sourceStream.Length,
                IsRenamed = payload.SourcePath.ModifiedPath != null,
            };

            await client.Entities.SignalEntityAsync(entityId, nameof(TransferEntityState.AddSuccessfulItem), successfulItem, null, cancellationToken);
        }
        catch (FileExistsException ex)
        {
            _logger.LogWarning(ex, "File already exists: {Path}", payload.SourcePath.Path);

            var failedItem = new TransferFailedItem
            {
                SourcePath = payload.SourcePath.Path,
                Status = TransferItemStatus.Failed,
                ErrorCode = TransferErrorCode.FileExists,
                ErrorMessage = ex.Message
            };

            await client.Entities.SignalEntityAsync(entityId, nameof(TransferEntityState.AddFailedItem), failedItem, null, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Transfer cancelled: {Path}", payload.SourcePath.Path);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transfer failed: {Path}", payload.SourcePath.Path);

            var failedItem = new TransferFailedItem
            {
                SourcePath = payload.SourcePath.Path,
                Status = TransferItemStatus.Failed,
                ErrorCode = TransferErrorCode.GeneralError,
                ErrorMessage = $"Exception: {ex.GetType().FullName}: {ex.Message}{Environment.NewLine}StackTrace: {ex.StackTrace}"
            };

            await client.Entities.SignalEntityAsync(entityId, nameof(TransferEntityState.AddFailedItem), failedItem, null, cancellationToken);
        }
    }
}
