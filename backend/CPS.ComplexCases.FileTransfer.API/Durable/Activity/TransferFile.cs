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
using CPS.ComplexCases.NetApp.Client;

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
        var startTime = DateTime.UtcNow;

        var (sourceClient, destinationClient) = _storageClientFactory.GetClientsForDirection(payload.TransferDirection);

        try
        {
            var sourceFilePath = string.IsNullOrEmpty(payload.SourcePath.ModifiedPath) ? payload.SourcePath.Path : payload.SourcePath.ModifiedPath;

            var (sourceStream, totalSize) = await sourceClient.OpenReadStreamAsync(
                payload.SourcePath.Path, payload.WorkspaceId, payload.SourcePath.FileId, payload.BearerToken, payload.BucketName);

            using (sourceStream)
            {
                const int oneMb = 1024 * 1024;
                const int minMultipartSize = 5 * oneMb;
                const int targetPartSize = 8 * oneMb;

                // Determine if we need MD5 computation (only for EgressStorageClient)
                bool needsMd5 = destinationClient is EgressStorageClient;

                // if the destination is NetApp we should avoid chunked upload for small files
                if (destinationClient is NetAppStorageClient && totalSize <= minMultipartSize)
                {
                    _logger.LogInformation("File size {TotalSize} <= {MinMultipartSize} bytes, using single PUT.",
                        totalSize, minMultipartSize);

                    await destinationClient.UploadFileAsync(payload.DestinationPath, sourceStream,
                        payload.WorkspaceId, sourceFilePath, payload.SourceRootFolderPath, payload.BearerToken, payload.BucketName);

                    var singleUpload = new TransferItem
                    {
                        SourcePath = payload.SourcePath.FullFilePath ?? payload.SourcePath.Path,
                        Status = TransferItemStatus.Completed,
                        Size = totalSize,
                        IsRenamed = payload.SourcePath.ModifiedPath != null,
                        FileId = payload.SourcePath.FileId
                    };

                    return new TransferResult
                    {
                        IsSuccess = true,
                        SuccessfulItem = singleUpload
                    };
                }

                // Multipart upload
                var session = await destinationClient.InitiateUploadAsync(
                    payload.DestinationPath, totalSize, sourceFilePath, payload.WorkspaceId,
                    payload.SourcePath.RelativePath, payload.SourceRootFolderPath, payload.BearerToken, payload.BucketName);

                long bytesProcessed = 0;
                long bytesUploaded = 0;
                int chunkNumber = 1;
                Dictionary<int, string> uploadedChunks = [];

                // Only initialize MD5 if needed
                System.Security.Cryptography.MD5? md5 = needsMd5 ? System.Security.Cryptography.MD5.Create() : null;
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

                    // Update MD5 hash as we read (only if needed)
                    if (md5 != null)
                    {
                        md5.TransformBlock(buffer, 0, bytesRead, null, 0);
                    }

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
                        var partBytes = partBuffer.ToArray();
                        long start = bytesUploaded;
                        long end = start + partBytes.Length - 1;

                        _logger.LogInformation("Uploading part {ChunkNumber}, bytes {Start}-{End} of {TotalSize}.",
                            chunkNumber, start, end, totalSize);

                        var result = await destinationClient.UploadChunkAsync(
                            session, chunkNumber, partBytes, start, end, totalSize, payload.BearerToken, payload.BucketName);

                        _logger.LogInformation("Uploaded part {ChunkNumber} with ETag {ETag} was successful.",
                            chunkNumber, result.ETag);

                        if (result.TransferDirection == TransferDirection.EgressToNetApp &&
                            result.PartNumber.HasValue && result.ETag != null)
                        {
                            uploadedChunks.Add(result.PartNumber.Value, result.ETag);
                        }

                        _logger.LogDebug("Transfer Id: {TransferId} Uploaded chunk: {ChunkNumber} ({Start}-{End}/{TotalSize})",
                            payload.TransferId, chunkNumber, start, end, totalSize);

                        bytesUploaded += partBytes.Length;
                        chunkNumber++;
                        partBuffer.SetLength(0);
                    }
                }

                // Finalize MD5 hash (only if we computed it)
                string md5Hash = string.Empty;
                if (md5 != null)
                {
                    md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                    md5Hash = md5.Hash != null ? Convert.ToBase64String(md5.Hash) : string.Empty;
                }

                // Complete the upload
                if (destinationClient is EgressStorageClient)
                {
                    await destinationClient.CompleteUploadAsync(session, md5hash: md5Hash);
                }
                else
                {
                    await destinationClient.CompleteUploadAsync(session, null, etags: uploadedChunks, payload.BearerToken, payload.BucketName);
                }

                _logger.LogInformation("File transfer completed: {SourcePath} -> {DestinationPath}",
                    payload.SourcePath.Path, payload.DestinationPath);

                var endTime = DateTime.UtcNow;

                var successfulItem = new TransferItem
                {
                    SourcePath = payload.SourcePath.FullFilePath ?? payload.SourcePath.Path,
                    Status = TransferItemStatus.Completed,
                    Size = totalSize,
                    IsRenamed = payload.SourcePath.ModifiedPath != null,
                    FileId = payload.SourcePath.FileId,
                    StartTime = startTime,
                    EndTime = endTime,
                };

                return new TransferResult
                {
                    IsSuccess = true,
                    SuccessfulItem = successfulItem
                };
            }
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