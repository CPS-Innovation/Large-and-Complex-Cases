using CPS.ComplexCases.Common.Models.Domain;

namespace CPS.ComplexCases.Common.Interfaces;

public interface IStorageClient
{
    Task<Stream> OpenReadStreamAsync(string path, string? workspaceId = null, string? fileId = null);
    Task<UploadSession> InitiateUploadAsync(string destinationPath, long fileSize, string? workspaceId = null, string? fileId = null);
    Task UploadChunkAsync(UploadSession session, int chunkNumber, byte[] chunkData, string? contentRange = null);
    Task CompleteUploadAsync(string? md5hash = null, List<string>? etags = null);
}