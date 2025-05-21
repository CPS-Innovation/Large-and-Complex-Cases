using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using Microsoft.Azure.Functions.Worker;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

public class ListSourceFiles
{
    [Function(nameof(UpdateTransferStatus))]
    public async Task<List<Guid>> Run([ActivityTrigger] ListSourceFilesPayload sourceFilesPayload, FunctionContext context)
    {
        // list files from egress/netapp

        // create transferItem records for each file 
        await Task.Delay(1000);

        return new List<Guid>();
    }
}