using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Activity;

public interface ITransferFile
{
    Task<TransferResult> Run(TransferFilePayload payload, CancellationToken cancellationToken = default);
}
