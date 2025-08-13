using Microsoft.Extensions.Options;
using CPS.ComplexCases.Common.Extensions;
using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Common.Models.Domain.Dtos;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Common.Storage;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;

namespace CPS.ComplexCases.NetApp.Client;

public class NetAppStorageClient(INetAppClient netAppClient, INetAppArgFactory netAppArgFactory, IOptions<NetAppOptions> options, ICaseMetadataService caseMetadataService) : IStorageClient
{
    private readonly INetAppClient _netAppClient = netAppClient;
    private readonly INetAppArgFactory _netAppArgFactory = netAppArgFactory;
    private readonly ICaseMetadataService _caseMetadataService = caseMetadataService;
    private readonly NetAppOptions _options = options.Value;

    public async Task CompleteUploadAsync(UploadSession session, string? md5hash = null, Dictionary<int, string>? etags = null)
    {
        var arg = _netAppArgFactory.CreateCompleteMultipartUploadArg(
            _options.BucketName,
            session.WorkspaceId ?? throw new ArgumentNullException(nameof(session.WorkspaceId), "session.WorkspaceId cannot be null."),
            session.UploadId ?? throw new ArgumentNullException(nameof(session.UploadId), "Upload ID cannot be null."),
            etags ?? throw new ArgumentNullException(nameof(etags), "ETags cannot be null."));

        var result = await _netAppClient.CompleteMultipartUploadAsync(arg);

        if (result == null)
            throw new InvalidOperationException("Failed to complete multipart upload.");
    }

    public async Task<UploadSession> InitiateUploadAsync(string destinationPath, long fileSize, string sourcePath, string? workspaceId = null, string? relativePath = null)
    {
        var fullDestinationPath = Path.Combine(destinationPath, sourcePath).Replace('\\', '/');

        var arg = _netAppArgFactory.CreateInitiateMultipartUploadArg(_options.BucketName, fullDestinationPath);

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

    public async Task<Stream> OpenReadStreamAsync(string path, string? workspaceId = null, string? fileId = null)
    {
        var arg = _netAppArgFactory.CreateGetObjectArg(_options.BucketName, path);
        var response = await _netAppClient.GetObjectAsync(arg);
        return response?.ResponseStream ?? throw new InvalidOperationException("Failed to get object stream.");
    }

    public async Task<UploadChunkResult> UploadChunkAsync(UploadSession session, int chunkNumber, byte[] chunkData, long? start = null, long? end = null, long? totalSize = null)
    {
        var arg = _netAppArgFactory.CreateUploadPartArg(
            _options.BucketName,
            session.WorkspaceId ?? throw new ArgumentNullException(nameof(session.WorkspaceId), "Workspace ID cannot be null."),
            chunkData,
            chunkNumber,
            session.UploadId ?? throw new ArgumentNullException(nameof(session.UploadId), "Upload ID cannot be null."));

        var result = await _netAppClient.UploadPartAsync(arg);

        if (result == null)
            throw new InvalidOperationException("Failed to upload part.");

        return new UploadChunkResult(TransferDirection.EgressToNetApp, result?.ETag, result?.PartNumber);
    }

    public async Task<IEnumerable<FileTransferInfo>> ListFilesForTransferAsync(List<TransferEntityDto> selectedEntities, string? workspaceId = null, int? caseId = null)
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
                var files = await ListFilesInFolder(path);
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

    public async Task<IEnumerable<FileTransferInfo>?> ListFilesInFolder(string path)
    {
        var filesForTransfer = new List<FileTransferInfo>();

        string? continuationToken = null;
        do
        {
            var arg = _netAppArgFactory.CreateListObjectsInBucketArg(_options.BucketName, continuationToken, 1000, path);
            var response = await _netAppClient.ListObjectsInBucketAsync(arg);

            if (response == null || !response.Data.FileData.Any())
            {
                return filesForTransfer;
            }

            filesForTransfer.AddRange(
                response.Data.FileData.Select(x => new FileTransferInfo
                {
                    SourcePath = x.Path
                }));

            continuationToken = response.Pagination.NextContinuationToken;
        } while (continuationToken != null);

        return filesForTransfer;
    }

    public Task<DeleteFilesResult> DeleteFilesAsync(List<DeletionEntityDto> filesToDelete, string? workspaceId = null)
    {
        throw new NotImplementedException();
    }
}