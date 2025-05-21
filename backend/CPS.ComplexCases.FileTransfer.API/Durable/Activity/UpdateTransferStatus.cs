using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Models.Domain;
using Microsoft.Azure.Functions.Worker;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

public class UpdateTransferStatus
{
    [Function(nameof(UpdateTransferStatus))]
    public async Task<Transfer> Run([ActivityTrigger] UpdateTransferStatusPayload updateStatusPayload, FunctionContext context)
    {
        // update transfer record in db with status
        await Task.Delay(1000);

        return new Transfer
        {
            Id = updateStatusPayload.TransferId,
            Status = updateStatusPayload.Status,
            DestinationPath = "destination/path",
            SourcePaths = new List<string> { "source/path1", "source/path2" },
            CaseId = 12,
        };
    }
}