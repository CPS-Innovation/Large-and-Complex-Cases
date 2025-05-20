using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.FileTransfer.API.Models.Responses;

namespace CPS.ComplexCases.FileTransfer.API.Services;

public interface ITransferService
{
    Task<TransferResponse> InitiateTransferAsync(Guid transferId, TransferRequest transferRequest, Guid correlationId);
}