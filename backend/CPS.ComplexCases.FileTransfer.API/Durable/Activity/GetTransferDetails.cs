using CPS.ComplexCases.FileTransfer.API.Models.Domain;
using Microsoft.Azure.Functions.Worker;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

public class GetTransferDetails
{
    [Function(nameof(GetTransferDetails))]
    public async Task<Transfer> Run([ActivityTrigger] Guid transferId, FunctionContext context)
    {
        // todo: get transfer out of db
        await Task.Delay(1000); // Simulate async db call

        return new Transfer
        {
            Id = transferId,
            Status = Models.Domain.Enums.TransferStatus.Initiated,
            DestinationPath = "destination/path",
            SourcePaths = new List<string> { "source/path1", "source/path2" },
            CaseId = 12,
        };

    }
}