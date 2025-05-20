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
    public async Task<TransferResponse> InitiateTransferAsync(Guid transferId, TransferRequest transferRequest, Guid correlationId)
    {
        try
        {
            // create new transfer record in db
            var transfer = new Transfer
            {
                Id = transferId,
                Status = TransferStatus.Initiated,
                CreatedAt = DateTime.UtcNow,
                DestinationPath = transferRequest.DestinationPath,
                // todo: how do we get overall sourcePath ? 
                SourcePath = transferRequest.SourcePaths[0],
                CaseId = transferRequest.Metadata.CaseId,
            };

            // create new audit record in db

            return new TransferResponse
            {
                Id = transfer.Id,
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