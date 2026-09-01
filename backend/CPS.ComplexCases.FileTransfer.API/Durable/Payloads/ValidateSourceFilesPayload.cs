using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Models.Requests;

namespace CPS.ComplexCases.FileTransfer.API.Durable.Payloads;

public class ValidateSourceFilesPayload
{
    public required TransferDirection TransferDirection { get; set; }
    public required List<TransferSourcePath> SourcePaths { get; set; }
    public required string WorkspaceId { get; set; }
    public required string BearerToken { get; set; }
    public required string BucketName { get; set; }
    public int CaseId { get; set; }
    public string? UserName { get; set; }
    public Guid? CorrelationId { get; set; }
}
