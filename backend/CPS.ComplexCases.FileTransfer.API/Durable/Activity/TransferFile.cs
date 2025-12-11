using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CPS.ComplexCases.Common.Models.Domain.Exceptions;
using CPS.ComplexCases.Egress.Client;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Factories;
using CPS.ComplexCases.FileTransfer.API.Models.Configuration;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.Common.Storage;
using CPS.ComplexCases.Common.Models.Domain;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

/// <summary>
/// Durable activity for transferring files between storage endpoints.
/// Handles chunked uploads, computes MD5 hashes for integrity, and returns transfer results.
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
            var sourceFilePath = string.IsNullOrEmpty(payload.SourcePath.ModifiedPath)
                ? payload.SourcePath.Path
                : payload.SourcePath.ModifiedPath;

            using var sourceStream = await sourceClient.OpenReadStreamAsync(
                payload.SourcePath.Path, payload.WorkspaceId, payload.SourcePath.FileId, payload.BearerToken);

            long totalSize = sourceStream.Length;
            bool needsMd5 = destinationClient is EgressStorageClient;
            bool isNetApp = destinationClient is NetAppStorageClient;

            // Handle small files with single PUT for NetApp
            if (isNetApp && totalSize <= _sizeConfig.MinMultipartSizeBytes)
            {
                return await HandleSingleUpload(sourceStream, destinationClient, payload, sourceFilePath, totalSize);
            }

            // All Egress files and large NetApp files use multipart upload
            return await HandleMultipartUpload(
                sourceStream, destinationClient, payload, sourceFilePath, totalSize, needsMd5, cancellationToken);
        }
        catch (FileExistsException ex)
        {
            return CreateFailureResult(payload.SourcePath.Path, TransferErrorCode.FileExists, ex.Message, ex);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Transfer cancelled: {Path}", payload.SourcePath.Path);
            throw;
        }
        catch (Exception ex)
        {
            return CreateFailureResult(payload.SourcePath.Path, TransferErrorCode.GeneralError,
                $"Exception: {ex.GetType().FullName}: {ex.Message}{Environment.NewLine}StackTrace: {ex.StackTrace}", ex);
        }
    }

    private async Task<TransferResult> HandleSingleUpload(
        Stream sourceStream,
        IStorageClient destinationClient,
        TransferFilePayload payload,
        string sourceFilePath,
        long totalSize)
    {
        _logger.LogInformation("File size {TotalSize} <= {MinMultipartSize} bytes, using single PUT.",
            totalSize, _sizeConfig.MinMultipartSizeBytes);

        if (sourceStream.CanSeek)
        {
            sourceStream.Position = 0;
        }

        await destinationClient.UploadFileAsync(
            payload.DestinationPath,
            sourceStream,
            payload.WorkspaceId,
            sourceFilePath,
            payload.SourceRootFolderPath,
            payload.BearerToken);

        var item = new TransferItem
        {
            SourcePath = payload.SourcePath.FullFilePath ?? payload.SourcePath.Path,
            Status = TransferItemStatus.Completed,
            Size = totalSize,
            IsRenamed = payload.SourcePath.ModifiedPath != null,
            FileId = payload.SourcePath.FileId
        };

        return new TransferResult { IsSuccess = true, SuccessfulItem = item };
    }

    private async Task<TransferResult> HandleMultipartUpload(
        Stream sourceStream,
        IStorageClient destinationClient,
        TransferFilePayload payload,
        string sourceFilePath,
        long totalSize,
        bool needsMd5,
        CancellationToken cancellationToken)
    {
        var session = await destinationClient.InitiateUploadAsync(
            payload.DestinationPath, totalSize, sourceFilePath, payload.WorkspaceId,
            payload.SourcePath.RelativePath, payload.SourceRootFolderPath, payload.BearerToken);

        if (sourceStream.CanSeek)
            sourceStream.Position = 0;

        var buffer = new byte[_sizeConfig.ChunkSizeBytes];
        var partList = new List<(int PartNumber, byte[] Data, long Start, long End)>();

        System.Security.Cryptography.MD5? md5 = needsMd5 ? System.Security.Cryptography.MD5.Create() : null;

        long bytesProcessed = 0;
        long absolutePosition = 0;
        int partNumber = 1;

        using (md5)
        {
            // STEP 1 — Sequentially READ & buffer all parts
            while (bytesProcessed < totalSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var partBuffer = new MemoryStream(_sizeConfig.ChunkSizeBytes);

                while (partBuffer.Length < _sizeConfig.ChunkSizeBytes && bytesProcessed < totalSize)
                {
                    int bytesToRead = (int)Math.Min(buffer.Length, totalSize - bytesProcessed);
                    int bytesRead = await sourceStream.ReadAsync(buffer.AsMemory(0, bytesToRead), cancellationToken);

                    if (bytesRead == 0)
                        break;

                    partBuffer.Write(buffer, 0, bytesRead);
                    md5?.TransformBlock(buffer, 0, bytesRead, null, 0);

                    bytesProcessed += bytesRead;
                }

                var arr = partBuffer.ToArray();
                long start = absolutePosition;
                long end = start + arr.Length - 1;
                absolutePosition += arr.Length;

                partList.Add((partNumber++, arr, start, end));
            }

            if (md5 != null)
            {
                md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
            }

            _logger.LogInformation("Buffered {Count} parts for parallel upload.", partList.Count);

            // STEP 2 — Parallel upload using limited concurrency
            var semaphore = new SemaphoreSlim(_sizeConfig.MaxConcurrentPartUploads);
            var uploadedEtags = new Dictionary<int, string>();

            var tasks = partList.Select(async part =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var (num, data, start, end) = part;

                    _logger.LogInformation("Parallel upload: part {Part}, bytes {Start}-{End}",
                        num, start, end);

                    var result = await destinationClient.UploadChunkAsync(
                        session, num, data, start, end, totalSize, payload.BearerToken);

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
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

            // STEP 3 — Complete upload
            string md5Hash = md5?.Hash != null ? Convert.ToBase64String(md5.Hash) : string.Empty;

            await CompleteUpload(destinationClient, session, md5Hash, uploadedEtags, payload.BearerToken);

            _logger.LogInformation(
                "Completed parallel multipart transfer for {Source} -> {Dest}",
                payload.SourcePath.Path, payload.DestinationPath
            );

            return CreateSuccessResult(payload, totalSize);
        }
    }

    private async Task CompleteUpload(
        IStorageClient destinationClient,
        UploadSession session,
        string md5Hash,
        Dictionary<int, string> uploadedChunks,
        string bearerToken)
    {
        if (destinationClient is EgressStorageClient)
        {
            await destinationClient.CompleteUploadAsync(session, md5hash: md5Hash);
        }
        else
        {
            await destinationClient.CompleteUploadAsync(session, null, etags: uploadedChunks, bearerToken);
        }
    }

    private TransferResult CreateSuccessResult(TransferFilePayload payload, long totalSize)
    {
        var item = new TransferItem
        {
            SourcePath = payload.SourcePath.FullFilePath ?? payload.SourcePath.Path,
            Status = TransferItemStatus.Completed,
            Size = totalSize,
            IsRenamed = payload.SourcePath.ModifiedPath != null,
            FileId = payload.SourcePath.FileId
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
}