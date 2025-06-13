using Microsoft.Extensions.Options;
using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Common.Models.Domain.Dtos;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Models.Domain.Exceptions;
using CPS.ComplexCases.Common.Storage;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;

namespace CPS.ComplexCases.NetApp.Client;

public class NetAppStorageClient(INetAppClient netAppClient, INetAppArgFactory netAppArgFactory, IOptions<NetAppOptions> options) : IStorageClient
{
    private readonly INetAppClient _netAppClient = netAppClient;
    private readonly INetAppArgFactory _netAppArgFactory = netAppArgFactory;
    private readonly NetAppOptions _options = options.Value;

    public async Task CompleteUploadAsync(UploadSession session, string? md5hash = null, Dictionary<int, string>? etags = null)
    {
        var arg = _netAppArgFactory.CreateCompleteMultipartUploadArg(
            _options.BucketName,
            session.WorkspaceId ?? throw new ArgumentNullException(nameof(session.WorkspaceId), "Workspace ID cannot be null."),
            session.UploadId ?? throw new ArgumentNullException(nameof(session.UploadId), "Upload ID cannot be null."),
            etags ?? throw new ArgumentNullException(nameof(etags), "ETags cannot be null."));

        await _netAppClient.CompleteMultipartUploadAsync(arg);
    }

    public async Task<UploadSession> InitiateUploadAsync(string destinationPath, long fileSize, string? workspaceId = null, string? sourcePath = null, TransferOverwritePolicy? overwritePolicy = null)
    {
        var arg = _netAppArgFactory.CreateInitiateMultipartUploadArg(_options.BucketName, destinationPath);
        if (overwritePolicy == null)
        {
            var getObjectArg = _netAppArgFactory.CreateGetObjectArg(_options.BucketName, destinationPath);
            var objectExists = await _netAppClient.DoesObjectExistAsync(getObjectArg);

            if (objectExists)
            {
                throw new FileExistsException($"Object {destinationPath} already exists.");
            }
        }
        var response = await _netAppClient.InitiateMultipartUploadAsync(arg);

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

    public async Task<UploadChunkResult> UploadChunkAsync(UploadSession session, int chunkNumber, byte[] chunkData, string? contentRange = null)
    {
        var arg = _netAppArgFactory.CreateUploadPartArg(
            _options.BucketName,
            session.WorkspaceId ?? throw new ArgumentNullException(nameof(session.WorkspaceId), "Workspace ID cannot be null."),
            chunkData,
            chunkNumber,
            session.UploadId ?? throw new ArgumentNullException(nameof(session.UploadId), "Upload ID cannot be null."));

        var result = await _netAppClient.UploadPartAsync(arg);

        return new UploadChunkResult(TransferDirection.EgressToNetApp, result?.ETag, result?.PartNumber);
    }

    public async Task<IEnumerable<FileTransferInfo>> ListFilesForTransferAsync(List<TransferEntityDto> selectedEntities, string? workspaceId = null)
    {
        List<FileTransferInfo> filesForTransfer = [];

        foreach (var entity in selectedEntities)
        {
            if (Path.HasExtension(entity.Path))
            {
                filesForTransfer.Add(new FileTransferInfo
                {
                    FilePath = entity.Path
                });
            }
            else
            {
                var files = await ListFilesInFolder(entity.Path);
                if (files != null)
                    filesForTransfer.AddRange(files);
            }
        }

        return filesForTransfer;
    }

    public async Task<IEnumerable<FileTransferInfo>?> ListFilesInFolder(string path, string? continuationToken = null)
    {
        var filesForTransfer = new List<FileTransferInfo>();
        var arg = _netAppArgFactory.CreateListObjectsInBucketArg(_options.BucketName, continuationToken, 1000, path);
        var response = await _netAppClient.ListObjectsInBucketAsync(arg);

        if (response == null || !response.Data.FileData.Any())
        {
            return null;
        }

        filesForTransfer.AddRange(
            response.Data.FileData.Select(x => new FileTransferInfo
            {
                FilePath = x.Path
            }));

        if (response.Pagination.NextContinuationToken != null)
        {
            var nextFiles = await ListFilesInFolder(path, response.Pagination.NextContinuationToken);
            if (nextFiles != null)
            {
                filesForTransfer.AddRange(nextFiles);
            }
        }

        return filesForTransfer;
    }
}