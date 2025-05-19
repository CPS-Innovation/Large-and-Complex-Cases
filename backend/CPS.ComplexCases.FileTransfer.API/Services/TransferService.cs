using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.FileTransfer.API.Models.Domain;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using CPS.ComplexCases.FileTransfer.API.Models.Responses;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.FileTransfer.API.Services;

public class TransferService : ITransferService
{
    private readonly ILogger<TransferService> _logger;

    public TransferService(ILogger<TransferService> logger)
    {
        _logger = logger;
    }
    public async Task<TransferResponse> InitiateTransferAsync(string instanceId, TransferRequest transferRequest, Guid correlationId)
    {
        try
        {
            // create new transfer record in db
            var transfer = new Transfer
            {
                TransferId = instanceId,
                Status = TransferStatus.Created,
                CreatedAt = DateTime.UtcNow,
                DestinationPath = transferRequest.DestinationPath,
                CaseId = transferRequest.Metadata.CaseId,
            };

            // create new audit record in db

            return new TransferResponse
            {
                TransferId = transfer.TransferId,
                Status = transfer.Status,
                CreatedAt = transfer.CreatedAt,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating transfer");
            throw;
        }
    }
}