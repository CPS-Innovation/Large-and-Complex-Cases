using CPS.ComplexCases.Common.Models.Domain;
using CPS.ComplexCases.Common.Models.Domain.Dtos;
using CPS.ComplexCases.Common.Models.Domain.Enums;

namespace CPS.ComplexCases.Common.Storage;

public interface IStorageClient
{
    Task<Stream> OpenReadStreamAsync(string path, string? workspaceId = null, string? fileId = null);
    Task<UploadSession> InitiateUploadAsync(string destinationPath, long fileSize, string? workspaceId = null, string? sourcePath = null, TransferOverwritePolicy? overwritePolicy = null);
    Task<UploadChunkResult> UploadChunkAsync(UploadSession session, int chunkNumber, byte[] chunkData, string? contentRange = null);
    Task CompleteUploadAsync(UploadSession session, string? md5hash = null, Dictionary<int, string>? etags = null);
    Task<IEnumerable<FileTransferInfo>> ListFilesForTransferAsync(List<TransferEntityDto> selectedEntities, string? workspaceId = null, int? caseId = null);
}