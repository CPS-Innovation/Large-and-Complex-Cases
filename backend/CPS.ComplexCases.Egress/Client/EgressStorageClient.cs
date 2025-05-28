using CPS.ComplexCases.Common.Interfaces;
using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.Egress.Models;
using CPS.ComplexCases.Egress.Models.Args;
using CPS.ComplexCases.Egress.Models.Response;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CPS.ComplexCases.Egress.Client;

public class EgressStorageClient(
    ILogger<EgressStorageClient> logger,
    IOptions<EgressOptions> egressOptions,
    HttpClient httpClient,
    IEgressRequestFactory egressRequestFactory) : BaseEgressClient(logger, egressOptions, httpClient, egressRequestFactory), IStorageClient
{
    public async Task<Stream> OpenReadStreamAsync(string path, string? workspaceId, string? fileId)
    {
        var token = await GetWorkspaceToken();

        var arg = new GetWorkspaceDocumentArg
        {
            WorkspaceId = workspaceId ?? throw new ArgumentNullException(nameof(workspaceId), "Workspace ID cannot be null."),
            FileId = fileId ?? throw new ArgumentNullException(nameof(fileId), "File ID cannot be null."),
        };

        var response = await SendRequestAsync(_egressRequestFactory.GetWorkspaceDocumentRequest(arg, token));
        return await response.Content.ReadAsStreamAsync();
    }
    public async Task<UploadSession> InitiateUploadAsync(string destinationPath, long fileSize, string? workspaceId = null, string? fileId = null)
    {
        var token = await GetWorkspaceToken();

        var fileName = Path.GetFileName(destinationPath);
        var arg = new CreateUploadArg
        {
            FolderPath = destinationPath,
            FileSize = fileSize,
            WorkspaceId = workspaceId ?? throw new ArgumentNullException(nameof(workspaceId), "Workspace ID cannot be null."),
            FileName = fileName
        };

        var response = await SendRequestAsync<CreateUploadResponse>(_egressRequestFactory.CreateUploadRequest(arg, token));

        return new UploadSession
        {
            UploadId = response.Id,
            WorkspaceId = workspaceId,
            Md5Hash = response.Md5Hash,
        };
    }

    public async Task UploadChunkAsync(UploadSession session, int chunkNumber, byte[] chunkData, string? contentRange = null)
    {
        var token = await GetWorkspaceToken();

        var uploadArg = new UploadChunkArg
        {
            UploadId = session.UploadId ?? throw new ArgumentNullException(nameof(session.UploadId), "Upload ID cannot be null."),
            WorkspaceId = session.WorkspaceId ?? throw new ArgumentNullException(nameof(session.WorkspaceId), "Workspace ID cannot be null."),
            ContentRange = contentRange,
            ChunkData = chunkData,
        };

        await SendRequestAsync(_egressRequestFactory.UploadChunkRequest(uploadArg, token));
    }
    public async Task CompleteUploadAsync(UploadSession session, string? md5hash = null, List<string>? etags = null)
    {
        var token = await GetWorkspaceToken();

        var completeArg = new CompleteUploadArg
        {
            UploadId = session.UploadId ?? throw new ArgumentNullException(nameof(session.UploadId), "Upload ID cannot be null."),
            WorkspaceId = session.WorkspaceId ?? throw new ArgumentNullException(nameof(session.WorkspaceId), "Workspace ID cannot be null."),
            Md5Hash = md5hash,
        };

        await SendRequestAsync(_egressRequestFactory.CompleteUploadRequest(completeArg, token));
    }
}