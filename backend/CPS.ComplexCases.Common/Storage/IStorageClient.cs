using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Common.Models.Domain.Dtos;

namespace CPS.ComplexCases.Common.Storage;

public interface IStorageClient
{
    Task<Stream> OpenReadStreamAsync(string path, string? workspaceId = null, string? fileId = null, string? bearerToken = null);
    Task<UploadSession> InitiateUploadAsync(string destinationPath, long fileSize, string sourcePath, string? workspaceId = null, string? relativePath = null, string? sourceRootFolderPath = null, string? bearerToken = null);
    Task<UploadChunkResult> UploadChunkAsync(UploadSession session, int chunkNumber, byte[] chunkData, long? start = null, long? end = null, long? totalSize = null, string? bearerToken = null);
    Task CompleteUploadAsync(UploadSession session, string? md5hash = null, Dictionary<int, string>? etags = null, string? bearerToken = null);
    Task<IEnumerable<FileTransferInfo>> ListFilesForTransferAsync(List<TransferEntityDto> selectedEntities, string? workspaceId = null, int? caseId = null, string? bearerToken = null);
    Task<DeleteFilesResult> DeleteFilesAsync(List<DeletionEntityDto> filesToDelete, string? workspaceId = null, string? bearerToken = null);
}