using CPS.ComplexCases.Common.Interfaces;
using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.Egress.Models;
using CPS.ComplexCases.Egress.Models.Args;
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
    public Task<UploadSession> InitiateUploadAsync(string destinationPath, long fileSize, string? workspaceId = null, string? fileId = null)
    {
        throw new NotImplementedException();
    }

    public Task UploadChunkAsync(UploadSession session, int chunkNumber, byte[] chunkData, string? contentRange = null)
    {
        throw new NotImplementedException();
    }
    public Task CompleteUploadAsync(UploadSession session, string? md5hash = null, List<string>? etags = null)
    {
        throw new NotImplementedException();
    }
}