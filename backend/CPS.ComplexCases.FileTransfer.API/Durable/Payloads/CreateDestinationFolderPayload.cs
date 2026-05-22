using CPS.ComplexCases.Common.Models.Domain.Enums;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Payloads;

public class CreateDestinationFolderPayload
{
    public int CaseId { get; set; }
    public string? UserName { get; set; }
    public Guid? CorrelationId { get; set; }
    public required string DestinationFolderPath { get; set; }
    public required string BearerToken { get; set; }
    public required string BucketName { get; set; }
    public required TransferDirection TransferDirection { get; set; }
}