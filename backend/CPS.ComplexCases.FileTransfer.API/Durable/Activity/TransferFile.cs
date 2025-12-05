using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Models.Domain.Exceptions;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Factories;
using CPS.ComplexCases.FileTransfer.API.Models.Configuration;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;


/// <summary>
/// Durable activity for transferring files between storage endpoints.
/// Handles chunked uploads, computes MD5 hashes for integrity, and returns transfer results.
/// No longer directly signals the entity - results are handled by the orchestrator.
/// </summary>
public class TransferFile(IStorageClientFactory storageClientFactory, ILogger<TransferFile> logger, IOptions<SizeConfig> sizeConfig)
{
    private readonly IStorageClientFactory _storageClientFactory = storageClientFactory;
    private readonly ILogger<TransferFile> _logger = logger;
    private readonly SizeConfig _sizeConfig = sizeConfig.Value;

    [Function(nameof(TransferFile))]
    public async Task<TransferResult> Run([ActivityTrigger] TransferFilePayload payload, CancellationToken cancellationToken = default)
    {
        var (sourceClient, destinationClient) = _storageClientFactory.GetClientsForDirection(payload.TransferDirection);

        try
        {
            var sourceFilePath = string.IsNullOrEmpty(payload.SourcePath.ModifiedPath) ? payload.SourcePath.Path : payload.SourcePath.ModifiedPath;

            using var sourceStream = await sourceClient.OpenReadStreamAsync(payload.SourcePath.Path, payload.WorkspaceId, payload.SourcePath.FileId);

            long totalSize = sourceStream.Length;
            const int oneMb = 1024 * 1024;
            const int minMultipartSize = 5 * oneMb; // S3 requires 5 MB minimum per part (except the last)
            const int targetPartSize = 8 * oneMb; // Aim for 8 MB aligned parts

            // If the file is small enough, avoid multipart entirely to prevent S3 validation errors
            if (totalSize <= minMultipartSize)
            {
                _logger.LogInformation("File size {TotalSize} <= {MinMultipartSize} bytes, using single PUT.",
                    totalSize, minMultipartSize);
                if (sourceStream.CanSeek)
                {
                    sourceStream.Position = 0;
                }

                await destinationClient.UploadFileAsync(payload.DestinationPath, sourceStream, payload.WorkspaceId, payload.SourcePath.RelativePath, payload.SourceRootFolderPath, payload.BearerToken);

                var singleUpload = new TransferItem
                {
                    SourcePath = payload.SourcePath.FullFilePath ?? payload.SourcePath.Path,
                    Status = TransferItemStatus.Completed,
                    Size = sourceStream.Length,
                    IsRenamed = payload.SourcePath.ModifiedPath != null,
                    FileId = payload.SourcePath.FileId
                };

                return new TransferResult
                {
                    IsSuccess = true,
                    SuccessfulItem = singleUpload
                };
            }

            var session = await destinationClient.InitiateUploadAsync(payload.DestinationPath, totalSize, sourceFilePath, payload.WorkspaceId, payload.SourcePath.RelativePath, payload.SourceRootFolderPath, payload.BearerToken);

            if (sourceStream.CanSeek)
            {
                sourceStream.Position = 0;
            }

            long bytesProcessed = 0;
            //int chunkSize = _sizeConfig.ChunkSizeBytes;
            int chunkNumber = 1;
            Dictionary<int, string> uploadedChunks = [];

            using var md5 = System.Security.Cryptography.MD5.Create();
            byte[] buffer = new byte[oneMb];
            using var partBuffer = new MemoryStream(targetPartSize);

            while (bytesProcessed < totalSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int bytesToRead = (int)Math.Min(buffer.Length, totalSize - bytesProcessed);
                int bytesRead = await sourceStream.ReadAsync(buffer.AsMemory(0, bytesToRead), cancellationToken);

                if (bytesRead <= 0)
                    break;

                partBuffer.Write(buffer, 0, bytesRead);
                bytesProcessed += bytesRead;

                long remaining = totalSize - bytesProcessed;
                bool isLastPart = remaining == 0;

                bool alignedToOneMb = partBuffer.Length % oneMb == 0;
                bool readyAtTarget = partBuffer.Length >= targetPartSize && alignedToOneMb;
                bool readyAtEnd = isLastPart && partBuffer.Length > 0;
                bool preventTinyRemainder = partBuffer.Length >= minMultipartSize && remaining > 0 &&
                                            remaining < minMultipartSize && alignedToOneMb;

                if (readyAtTarget || readyAtEnd || preventTinyRemainder)
                {
                    long start = bytesProcessed;
                    long end = start + bytesRead - 1;

                    md5.TransformBlock(buffer, 0, bytesRead, null, 0);

                    _logger.LogInformation("Uploading part {ChunkNumber}, bytes {Start}-{End} of {TotalSize}.", chunkNumber, start, end, totalSize);
                    var result = await destinationClient.UploadChunkAsync(session, chunkNumber, buffer[..bytesRead], start, end, totalSize, payload.BearerToken);
                    _logger.LogInformation("Uploaded part {ChunkNumber} with ETag {ETag} was successful.", chunkNumber, result.ETag);

                    if (result.TransferDirection == TransferDirection.EgressToNetApp && result.PartNumber.HasValue && result.ETag != null)
                    {
                        uploadedChunks.Add(result.PartNumber.Value, result.ETag);
                    }

                    _logger.LogDebug("Transfer Id: {TransferId} Uploaded chunk: {ChunkNumber} ({Start}-{End}/{TotalSize})", payload.TransferId, chunkNumber, start, end, totalSize);

                    bytesProcessed += bytesRead;
                    chunkNumber++;
                }
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
                await destinationClient.CompleteUploadAsync(session, null, etags: uploadedChunks, payload.BearerToken);
            }

            _logger.LogInformation("File transfer completed: {SourcePath} -> {DestinationPath}", payload.SourcePath.Path, payload.DestinationPath);

            var successfulItem = new TransferItem
            {
                SourcePath = payload.SourcePath.FullFilePath ?? payload.SourcePath.Path,
                Status = TransferItemStatus.Completed,
                Size = sourceStream.Length,
                IsRenamed = payload.SourcePath.ModifiedPath != null,
                FileId = payload.SourcePath.FileId
            };

            return new TransferResult
            {
                IsSuccess = true,
                SuccessfulItem = successfulItem
            };
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

            return new TransferResult
            {
                IsSuccess = false,
                FailedItem = failedItem
            };
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Transfer cancelled: {Path}", payload.SourcePath.Path);
            throw; // Re-throw cancellation exceptions
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

            return new TransferResult
            {
                IsSuccess = false,
                FailedItem = failedItem
            };
        }
    }
}