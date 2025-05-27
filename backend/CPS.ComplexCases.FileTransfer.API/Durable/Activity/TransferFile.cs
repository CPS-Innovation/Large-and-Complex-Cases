using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using CPS.ComplexCases.FileTransfer.API.Models.Domain;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.DurableTask.Entities;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

public class TransferFile
{
    [Function(nameof(TransferFile))]
    public async Task Run([ActivityTrigger] TransferFilePayload transferFilePayload, [DurableClient] DurableTaskClient client)
    {
        // get the source Stream

        // initalise multi part upload

        // update state with any failed items
    }
}