using System.Buffers;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CPS.ComplexCases.Common.Extensions;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Models.Domain.Exceptions;
using CPS.ComplexCases.Common.Storage;
using CPS.ComplexCases.Common.Telemetry;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Factories;
using CPS.ComplexCases.FileTransfer.API.Models.Configuration;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using CPS.ComplexCases.FileTransfer.API.Telemetry;
using CPS.ComplexCases.NetApp.Client;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

/// <summary>
/// Durable activity for transferring files between storage endpoints.
/// Handles chunked uploads, computes MD5 hashes for integrity, and returns transfer results.
/// </summary>
public class TransferFile(IStorageClientFactory storageClientFactory, ILogger<TransferFile> logger, IOptions<SizeConfig> sizeConfig, IInitializationHandler initializationHandler, ITelemetryClient telemetryClient)
{
    private readonly IStorageClientFactory _storageClientFactory = storageClientFactory;
    private readonly ILogger<TransferFile> _logger = logger;
    private readonly SizeConfig _sizeConfig = sizeConfig.Value;
    private readonly IInitializationHandler _initializationHandler = initializationHandler;
    private readonly ITelemetryClient _telemetryClient = telemetryClient;

    [Function(nameof(TransferFile))]
    public async Task<TransferResult> Run([ActivityTrigger] TransferFilePayload payload, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;

        _initializationHandler.Initialize(payload.UserName, payload.CorrelationId);

        var (sourceClient, destinationClient) = _storageClientFactory.GetClientsForDirection(payload.TransferDirection);

        var telemetryEvent = new FileTransferEvent
        {
            CaseId = payload.CaseId,
            TransferStartTime = startTime,
        };

        try
        {
            var sourceFilePath = string.IsNullOrEmpty(payload.SourcePath.ModifiedPath)
                ? payload.SourcePath.Path
                : payload.SourcePath.ModifiedPath;

            if (payload.TransferDirection == TransferDirection.EgressToNetApp)
            {
                var existingFilepath = GetDestinationPath(payload);

                var fileExists = await destinationClient.FileExistsAsync(
                    existingFilepath,
                    payload.WorkspaceId,
                    payload.BearerToken,
                    payload.BucketName);

                if (fileExists)
                {
                    throw new FileExistsException($"File already exists at destination path: {existingFilepath}");
                }
            }

            var (sourceStream, totalSize) = await sourceClient.OpenReadStreamAsync(
                payload.SourcePath.Path,
                payload.WorkspaceId,
                payload.SourcePath.FileId,
                payload.BearerToken,
                payload.BucketName);

            using (sourceStream)
            {
                bool needsMd5 = destinationClient is EgressStorageClient;
                bool isNetApp = destinationClient is NetAppStorageClient;

                TransferResult result;

                // Small NetApp files use single PUT
                if (isNetApp && totalSize <= _sizeConfig.MinMultipartSizeBytes)
                {
                    result = await HandleSingleUpload(
                        sourceStream,
                        destinationClient,
                        payload,
                        sourceFilePath,
                        totalSize,
                        startTime);
                }
                else
                {
                    // All Egress + large NetApp files use multipart upload
                    result = await HandleMultipartUpload(
                        sourceStream,
                        destinationClient,
                        payload,
                        sourceFilePath,
                        totalSize,
                        needsMd5,
                        cancellationToken,
                        startTime);
                }

                telemetryEvent.FileSizeInBytes = totalSize;
                telemetryEvent.TransferEndTime = result.SuccessfulItem?.EndTime ?? DateTime.UtcNow;
                telemetryEvent.IsSuccessful = result.IsSuccess;
                telemetryEvent.IsMultipart = result.IsSuccess && result.SuccessfulItem!.TotalPartsCount > 1;
                telemetryEvent.TotalPartsCount = result.IsSuccess ? result.SuccessfulItem!.TotalPartsCount : 0;

                if (!result.IsSuccess && result.FailedItem != null)
                {
                    telemetryEvent.ErrorCode = result.FailedItem.ErrorCode.ToString();
                    telemetryEvent.ErrorMessage = result.FailedItem.ErrorMessage;
                }

                _telemetryClient.TrackEvent(telemetryEvent);

                return result;
            }
        }
        catch (FileExistsException ex)
        {
            LogFileConflictTelemetry(payload);
            telemetryEvent.ErrorCode = TransferErrorCode.FileExists.ToString();
            telemetryEvent.ErrorMessage = ex.Message;
            return CreateFailureResult(payload.SourcePath.Path, TransferErrorCode.FileExists, ex.Message, ex);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            var errorMessage = $"HTTP request timed out: {ex.Message}";
            _logger.LogWarning(ex, "HTTP request timed out during transfer: {Path}", payload.SourcePath.Path);
            telemetryEvent.ErrorCode = TransferErrorCode.GeneralError.ToString();
            telemetryEvent.ErrorMessage = errorMessage;
            return CreateFailureResult(
                payload.SourcePath.Path,
                TransferErrorCode.GeneralError,
                errorMessage,
                ex);
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogInformation(ex, "Transfer cancelled: {Path}", payload.SourcePath.Path);
            throw;
        }
        catch (Exception ex)
        {
            var errorMessage = $"Exception: {ex.GetType().FullName}: {ex.Message}{Environment.NewLine}StackTrace: {ex.StackTrace}";
            telemetryEvent.ErrorCode = TransferErrorCode.GeneralError.ToString();
            telemetryEvent.ErrorMessage = errorMessage;
            return CreateFailureResult(
                payload.SourcePath.Path,
                TransferErrorCode.GeneralError,
                errorMessage,
                ex);
        }
        finally
        {
            telemetryEvent.TransferEndTime = DateTime.UtcNow;
            _telemetryClient.TrackEvent(telemetryEvent);
        }
    }

    private async Task<TransferResult> HandleSingleUpload(
        Stream sourceStream,
        IStorageClient destinationClient,
        TransferFilePayload payload,
        string sourceFilePath,
        long totalSize,
        DateTime startTime)
    {
        _logger.LogInformation(
            "File size {TotalSize} <= {MinMultipartSize} bytes, using single PUT.",
            totalSize,
            _sizeConfig.MinMultipartSizeBytes);

        await destinationClient.UploadFileAsync(
            payload.DestinationPath,
            sourceStream,
            totalSize,
            payload.WorkspaceId,
            sourceFilePath,
            payload.SourceRootFolderPath,
            payload.BearerToken,
            payload.BucketName);

        return CreateSuccessResult(payload, totalSize, startTime);
    }

    private async Task<TransferResult> HandleMultipartUpload(
        Stream sourceStream,
        IStorageClient destinationClient,
        TransferFilePayload payload,
        string sourceFilePath,
        long totalSize,
        bool needsMd5,
        CancellationToken cancellationToken,
        DateTime startTime)
    {
        var session = await destinationClient.InitiateUploadAsync(
            payload.DestinationPath,
            totalSize,
            sourceFilePath,
            payload.WorkspaceId,
            payload.SourcePath.RelativePath,
            payload.SourceRootFolderPath,
            payload.BearerToken,
            payload.BucketName);

        var uploadSemaphore = new SemaphoreSlim(_sizeConfig.MaxConcurrentPartUploads);
        var uploadedEtags = new Dictionary<int, string>();
        var uploadTasks = new List<Task>();

        System.Security.Cryptography.MD5? md5 = needsMd5 ? System.Security.Cryptography.MD5.Create() : null;
        long bytesProcessed = 0;
        int partNumber = 1;

        using (md5)
        {
            while (bytesProcessed < totalSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                long remainingBytes = totalSize - bytesProcessed;
                int targetPartSize = (int)Math.Min(_sizeConfig.ChunkSizeBytes, remainingBytes);
                var partData = ArrayPool<byte>.Shared.Rent(targetPartSize);
                try
                {
                    int partOffset = 0;

                    while (partOffset < targetPartSize)
                    {
                        int bytesToRead = targetPartSize - partOffset;
                        int bytesRead = await sourceStream.ReadAsync(
                            partData.AsMemory(partOffset, bytesToRead),
                            cancellationToken);

                        if (bytesRead == 0)
                        {
                            if (bytesProcessed + partOffset < totalSize)
                            {
                                throw new InvalidOperationException(
                                    $"Unexpected end of stream at position {bytesProcessed + partOffset}");
                            }
                            break;
                        }
                        partOffset += bytesRead;
                    }

                    if (md5 != null)
                    {
                        if (bytesProcessed + partOffset == totalSize)
                            md5.TransformFinalBlock(partData, 0, partOffset);
                        else
                            md5.TransformBlock(partData, 0, partOffset, null, 0);
                    }

                    long start = bytesProcessed;
                    long end = start + partOffset - 1;
                    bytesProcessed += partOffset;
                    int currentPartNumber = partNumber++;

                    // Wait for upload slot BEFORE copying data
                    await uploadSemaphore.WaitAsync(cancellationToken);
                    bool uploadTaskOwnsSemaphore = false;
                    try
                    {
                        // Copy part data for the upload task
                        var partDataCopy = partData.AsMemory(0, partOffset).ToArray();

                        // Fire and forget upload (semaphore already acquired)
                        var uploadTask = Task.Run(async () =>
                        {
                            try
                            {
                                _logger.LogInformation(
                                    "Uploading part {Part}, bytes {Start}-{End}/{Total}",
                                    currentPartNumber, start, end, totalSize);

                                var result = await destinationClient.UploadChunkAsync(
                                    session, currentPartNumber, partDataCopy,
                                    start, end, totalSize,
                                    payload.BearerToken, payload.BucketName);

                                if (result.PartNumber.HasValue && result.ETag != null)
                                {
                                    lock (uploadedEtags)
                                    {
                                        uploadedEtags[result.PartNumber.Value] = result.ETag;
                                    }
                                }
                            }
                            finally
                            {
                                uploadSemaphore.Release();
                            }
                        }, cancellationToken);

                        uploadTaskOwnsSemaphore = true;
                        uploadTasks.Add(uploadTask);
                    }
                    finally
                    {
                        if (!uploadTaskOwnsSemaphore)
                        {
                            uploadSemaphore.Release();
                        }
                    }
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(partData);
                }
            }

            await Task.WhenAll(uploadTasks);

            string md5Hash = md5?.Hash != null ? Convert.ToBase64String(md5.Hash) : string.Empty;
            string? filePath = payload.TransferDirection == TransferDirection.EgressToNetApp ? payload.DestinationPath.EnsureTrailingSlash() + payload.SourcePath.Path : null;
            var isVerified = await CompleteUpload(destinationClient, session, md5Hash, uploadedEtags,
                payload.BearerToken, payload.BucketName, filePath);

            if (!isVerified)
            {
                _logger.LogError("Upload completed but failed to verify upload for {Source} -> {Dest}",
                    payload.SourcePath.Path, payload.DestinationPath);

                return CreateFailureResult(
                    payload.SourcePath.Path,
                    TransferErrorCode.IntegrityVerificationFailed,
                    "Upload completed but failed to verify.");
            }

            _logger.LogInformation("Completed parallel multipart transfer for {Source} -> {Dest}",
                payload.SourcePath.Path, payload.DestinationPath);

            return CreateSuccessResult(payload, totalSize, startTime, partNumber);
        }
    }

    private static async Task<bool> CompleteUpload(
        IStorageClient destinationClient,
        UploadSession session,
        string md5Hash,
        Dictionary<int, string> uploadedChunks,
        string bearerToken,
        string? bucketName,
        string? filePath)
    {
        if (destinationClient is EgressStorageClient)
        {
            return await destinationClient.CompleteUploadAsync(session, md5hash: md5Hash);
        }
        else
        {
            return await destinationClient.CompleteUploadAsync(session, null, etags: uploadedChunks, bearerToken, bucketName, filePath);
        }
    }

    private static TransferResult CreateSuccessResult(TransferFilePayload payload, long totalSize, DateTime startTime, int totalParts = 1)
    {
        var endTime = DateTime.UtcNow;

        var item = new TransferItem
        {
            SourcePath = payload.SourcePath.FullFilePath ?? payload.SourcePath.Path,
            Status = TransferItemStatus.Completed,
            Size = totalSize,
            IsRenamed = payload.SourcePath.ModifiedPath != null,
            FileId = payload.SourcePath.FileId,
            StartTime = startTime,
            EndTime = endTime,
            TotalPartsCount = totalParts
        };

        return new TransferResult { IsSuccess = true, SuccessfulItem = item };
    }

    private TransferResult CreateFailureResult(
        string sourcePath,
        TransferErrorCode errorCode,
        string errorMessage,
        Exception? ex = null)
    {
        if (ex != null)
        {
            _logger.LogError(ex, "Transfer failed: {Path}", sourcePath);
        }
        else
        {
            _logger.LogWarning("Transfer failed: {Path} - {ErrorMessage}", sourcePath, errorMessage);
        }

        var failedItem = new TransferFailedItem
        {
            SourcePath = sourcePath,
            Status = TransferItemStatus.Failed,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage
        };

        return new TransferResult { IsSuccess = false, FailedItem = failedItem };
    }

    private static string GetDestinationPath(TransferFilePayload payload)
    {
        if (payload.TransferDirection == TransferDirection.EgressToNetApp)
        {
            return payload.DestinationPath + payload.SourcePath.Path;
        }
        else
        {
            int? index = payload.SourcePath.RelativePath?.IndexOf(payload.SourceRootFolderPath ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            if (index.HasValue && index.Value == 0 && !string.IsNullOrEmpty(payload.SourceRootFolderPath))
            {
                return payload.DestinationPath + payload.SourcePath.RelativePath!.Substring(payload.SourceRootFolderPath.Length).TrimStart('/', '\\');
            }
            else
            {
                return payload.DestinationPath + payload.SourcePath.RelativePath;
            }
        }
    }

    private void LogFileConflictTelemetry(TransferFilePayload payload)
    {
        var conflictEvent = new DuplicateFileConflictEvent
        {
            CaseId = payload.CaseId,
            SourceFilePath = payload.SourcePath.FullFilePath ?? payload.SourcePath.Path,
            DestinationFilePath = payload.DestinationPath + payload.SourcePath.Path,
            ConflictingFileName = Path.GetFileName(payload.SourcePath.Path),
            TransferDirection = payload.TransferDirection.ToString(),
            TransferId = payload.TransferId.ToString()
        };

        _telemetryClient.TrackEvent(conflictEvent);
    }
}
