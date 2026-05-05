using CPS.ComplexCases.Common.Models.Requests;

namespace CPS.ComplexCases.API.Clients.FileTransfer;

public interface IFileTransferClient
{
    Task<HttpResponseMessage> InitiateFileTransferAsync(TransferRequest transferRequest, Guid correlationId);
    Task<HttpResponseMessage> ListFilesForTransferAsync(ListFilesForTransferRequest request, Guid correlationId);
    Task<HttpResponseMessage> GetFileTransferStatusAsync(string transferId, Guid correlationId);
    Task<HttpResponseMessage> RenameNetAppMaterialAsync(RenameNetAppMaterialRequest request, Guid correlationId);
    Task<HttpResponseMessage> InitiateBatchCopyAsync(CopyNetAppBatchRequest request, Guid correlationId);
}