using CPS.ComplexCases.Common.Models.Requests;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;

public class ValidateSourceFilesResult
{
    public List<TransferSourcePath> Available { get; set; } = [];
    public List<TransferSourcePath> Missing { get; set; } = [];
    public List<TransferFailedItem> Failed { get; set; } = [];
}
