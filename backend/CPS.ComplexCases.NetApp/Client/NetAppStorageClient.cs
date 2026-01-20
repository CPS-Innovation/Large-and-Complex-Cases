using CPS.ComplexCases.Common.Extensions;
using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Common.Models.Domain.Dtos;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Common.Storage;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Streams;

namespace CPS.ComplexCases.NetApp.Client;

public class NetAppStorageClient(INetAppClient netAppClient, INetAppArgFactory netAppArgFactory, ICaseMetadataService caseMetadataService) : IStorageClient
{
    private readonly INetAppClient _netAppClient = netAppClient;
    private readonly INetAppArgFactory _netAppArgFactory = netAppArgFactory;
    private readonly ICaseMetadataService _caseMetadataService = caseMetadataService;

    public async Task CompleteUploadAsync(UploadSession session, string? md5hash = null, Dictionary<int, string>? etags = null, string? bearerToken = null, string? bucketName = null)
    {
        var arg = _netAppArgFactory.CreateCompleteMultipartUploadArg(
            bearerToken ?? throw new ArgumentNullException(nameof(bearerToken), "Bearer token cannot be null."),
            bucketName ?? throw new ArgumentNullException(nameof(bucketName), "Bucket name cannot be null."),
            session.WorkspaceId ?? throw new ArgumentNullException(nameof(session.WorkspaceId), "session.WorkspaceId cannot be null."),
            session.UploadId ?? throw new ArgumentNullException(nameof(session.UploadId), "Upload ID cannot be null."),
            etags ?? throw new ArgumentNullException(nameof(etags), "ETags cannot be null."));

        var result = await _netAppClient.CompleteMultipartUploadAsync(arg);

        if (result == null)
            throw new InvalidOperationException("Failed to complete multipart upload.");
    }

    public async Task<UploadSession> InitiateUploadAsync(string destinationPath, long fileSize, string sourcePath, string? workspaceId = null, string? relativePath = null, string? sourceRootFolderPath = null, string? bearerToken = null, string? bucketName = null)
    {
        var fullDestinationPath = Path.Combine(destinationPath, sourcePath).Replace('\\', '/');

        var arg = _netAppArgFactory.CreateInitiateMultipartUploadArg(
            bearerToken ?? throw new ArgumentNullException(nameof(bearerToken), "Bearer token cannot be null."),
            bucketName ?? throw new ArgumentNullException(nameof(bucketName), "Bucket name cannot be null."),
            fullDestinationPath);

        var response = await _netAppClient.InitiateMultipartUploadAsync(arg);

        if (response == null)
        {
            throw new InvalidOperationException("Failed to initiate multipart upload. Response was null.");
        }

        return new UploadSession
        {
            UploadId = response.UploadId,
            WorkspaceId = response.Key,
            Md5Hash = response.ServerSideEncryptionCustomerProvidedKeyMD5
        };
    }

    public async Task<(Stream Stream, long ContentLength)> OpenReadStreamAsync(string path, string? workspaceId = null, string? fileId = null, string? bearerToken = null, string? bucketName = null)
    {
        var arg = _netAppArgFactory.CreateGetObjectArg(
            bearerToken ?? throw new ArgumentNullException(nameof(bearerToken), "Bearer token cannot be null."),
            bucketName ?? throw new ArgumentNullException(nameof(bucketName), "Bucket name cannot be null."),
            path);

        var response = await _netAppClient.GetObjectAsync(arg);

        if (response?.ResponseStream == null)
            throw new InvalidOperationException("Failed to get object stream.");

        long contentLength = response.ContentLength;

        // Wrap the stream to safely handle AWS SDK's hash validation exception on disposal
        var stream = new HashValidationIgnoringStream(response.ResponseStream);
        return (stream, contentLength);
    }

    public async Task<UploadChunkResult> UploadChunkAsync(UploadSession session, int chunkNumber, byte[] chunkData, long? start = null, long? end = null, long? totalSize = null, string? bearerToken = null, string? bucketName = null)
    {
        var arg = _netAppArgFactory.CreateUploadPartArg(
            bearerToken ?? throw new ArgumentNullException(nameof(bearerToken), "Bearer token cannot be null."),
            bucketName ?? throw new ArgumentNullException(nameof(bucketName), "Bucket name cannot be null."),
            session.WorkspaceId ?? throw new ArgumentNullException(nameof(session.WorkspaceId), "Workspace ID cannot be null."),
            chunkData,
            chunkNumber,
            session.UploadId ?? throw new ArgumentNullException(nameof(session.UploadId), "Upload ID cannot be null."));

        var result = await _netAppClient.UploadPartAsync(arg);

        if (result == null)
            throw new InvalidOperationException("Failed to upload part.");

        return new UploadChunkResult(TransferDirection.EgressToNetApp, result?.ETag, result?.PartNumber);
    }

    public async Task<IEnumerable<FileTransferInfo>> ListFilesForTransferAsync(List<TransferEntityDto> selectedEntities, string? workspaceId = null, int? caseId = null, string? bearerToken = null, string? bucketName = null)
    {
        List<FileTransferInfo> filesForTransfer = [];

        var caseMetaData = await _caseMetadataService.GetCaseMetadataForCaseIdAsync(caseId ??
            throw new ArgumentNullException(nameof(caseId), "Case ID cannot be null.")) ??
                throw new InvalidOperationException($"No metadata found for case ID {caseId}.");

        var paths = selectedEntities.Select(entity => entity.Path);

        foreach (var path in paths)
        {
            if (Path.HasExtension(path))
            {
                filesForTransfer.Add(new FileTransferInfo
                {
                    SourcePath = path,
                    RelativePath = path.RemovePathPrefix(caseMetaData.NetappFolderPath),
                });
            }
            else
            {
                var files = await ListFilesInFolder(
                    path,
                    bearerToken ?? throw new ArgumentNullException(nameof(bearerToken), "Bearer token cannot be null."),
                    bucketName ?? throw new ArgumentNullException(nameof(bucketName), "Bucket name cannot be null."));

                if (files != null)
                    filesForTransfer.AddRange(files.Select(file => new FileTransferInfo
                    {
                        SourcePath = file.SourcePath,
                        RelativePath = file.SourcePath.RemovePathPrefix(caseMetaData.NetappFolderPath)
                    }));
            }
        }

        return filesForTransfer;
    }

    public async Task<IEnumerable<FileTransferInfo>?> ListFilesInFolder(string path, string bearerToken, string bucketName)
    {
        var filesForTransfer = new List<FileTransferInfo>();

        string? continuationToken = null;
        do
        {
            var arg = _netAppArgFactory.CreateListObjectsInBucketArg(
                bearerToken,
                bucketName,
                continuationToken,
                1000,
                path);

            var response = await _netAppClient.ListObjectsInBucketAsync(arg);

            if (response == null)
            {
                return filesForTransfer;
            }

            // Add all files from current level
            if (response.Data.FileData.Any())
            {
                filesForTransfer.AddRange(
                    response.Data.FileData.Select(x => new FileTransferInfo
                    {
                        SourcePath = x.Path
                    }));
            }

            // Recursively process all subdirectories
            if (response.Data.FolderData.Any())
            {
                foreach (var folder in response.Data.FolderData)
                {
                    if (!string.IsNullOrEmpty(folder.Path))
                    {
                        var subFiles = await ListFilesInFolder(folder.Path, bearerToken, bucketName);
                        if (subFiles != null)
                        {
                            filesForTransfer.AddRange(subFiles);
                        }
                    }
                }
            }

            continuationToken = response.Pagination.NextContinuationToken;
        } while (continuationToken != null);

        return filesForTransfer;
    }

    public Task<DeleteFilesResult> DeleteFilesAsync(List<DeletionEntityDto> filesToDelete, string? workspaceId = null, string? BearerToken = null, string? bucketName = null)
    {
        throw new NotImplementedException();
    }

    public async Task UploadFileAsync(string destinationPath, Stream fileStream, long contentLength, string? workspaceId = null, string? relativePath = null, string? sourceRootFolderPath = null, string? bearerToken = null, string? bucketName = null)
    {
        var objectName = Path.Combine(destinationPath, relativePath ?? string.Empty).Replace('\\', '/');

        var arg = _netAppArgFactory.CreateUploadObjectArg(
            bearerToken ?? throw new ArgumentNullException(nameof(bearerToken), "Bearer token cannot be null."),
            bucketName ?? throw new ArgumentNullException(nameof(bucketName), "Bucket name cannot be null."),
            objectName,
            fileStream,
            contentLength);

        var result = await _netAppClient.UploadObjectAsync(arg);

        if (!result)
            throw new InvalidOperationException($"Failed to upload {objectName} to NetApp.");
    }
}