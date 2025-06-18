using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;

namespace CPS.ComplexCases.FileTransfer.API.Models.Responses
{
    public class TransferResponse
    {
        public Guid Id { get; set; }
        public TransferStatus Status { get; set; }
        public required DateTime CreatedAt { get; set; }
    }
}