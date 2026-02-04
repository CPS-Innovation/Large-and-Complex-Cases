using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Common.Models.Domain.Dtos;

namespace CPS.ComplexCases.Common.Storage;

public interface IStorageClient
{
    Task<(Stream Stream, long ContentLength)> OpenReadStreamAsync(string path, string? workspaceId = null, string? fileId = null, string? bearerToken = null, string? bucketName = null);
    Task<UploadSession> InitiateUploadAsync(string destinationPath, long fileSize, string sourcePath, string? workspaceId = null, string? relativePath = null, string? sourceRootFolderPath = null, string? bearerToken = null, string? bucketName = null);
    Task<UploadChunkResult> UploadChunkAsync(UploadSession session, int chunkNumber, byte[] chunkData, long? start = null, long? end = null, long? totalSize = null, string? bearerToken = null, string? bucketName = null);
    Task<bool> CompleteUploadAsync(UploadSession session, string? md5hash = null, Dictionary<int, string>? etags = null, string? bearerToken = null, string? bucketName = null, string? filePath = null);
    Task UploadFileAsync(string destinationPath, Stream fileStream, long contentLength, string? workspaceId = null, string? relativePath = null, string? sourceRootFolderPath = null, string? bearerToken = null, string? bucketName = null);
    Task<IEnumerable<FileTransferInfo>> ListFilesForTransferAsync(List<TransferEntityDto> selectedEntities, string? workspaceId = null, int? caseId = null, string? bearerToken = null, string? bucketName = null);
    Task<DeleteFilesResult> DeleteFilesAsync(List<DeletionEntityDto> filesToDelete, string? workspaceId = null, string? bearerToken = null, string? bucketName = null);
    Task<bool> FileExistsAsync(string path, string? workspaceId = null, string? bearerToken = null, string? bucketName = null);
}