using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Common.Models.Domain.Dtos;

namespace CPS.ComplexCases.Common.Storage;

public interface IStorageClient
{
    Task<Stream> OpenReadStreamAsync(string path, string? workspaceId = null, string? fileId = null);
    Task<UploadSession> InitiateUploadAsync(string destinationPath, long fileSize, string sourcePath, string? workspaceId = null, string? relativePath = null);
    Task<UploadChunkResult> UploadChunkAsync(UploadSession session, int chunkNumber, byte[] chunkData, long? start = null, long? end = null, long? totalSize = null);
    Task CompleteUploadAsync(UploadSession session, string? md5hash = null, Dictionary<int, string>? etags = null);
    Task<IEnumerable<FileTransferInfo>> ListFilesForTransferAsync(List<TransferEntityDto> selectedEntities, string? workspaceId = null, int? caseId = null);
    Task<DeleteFilesResult> DeleteFilesAsync(List<DeletionEntityDto> filesToDelete, string? workspaceId = null);
}