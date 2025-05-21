using CPS.ComplexCases.Common.Models.Domain.Enums;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Payloads;

public class ListSourceFilesPayload
{
    public Guid TransferId { get; set; }
    public List<string> SourcePaths { get; set; } = new List<string>();
    public TransferDirection Direction { get; set; }
}