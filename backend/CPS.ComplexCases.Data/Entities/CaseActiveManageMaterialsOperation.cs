namespace CPS.ComplexCases.Data.Entities;

public class CaseActiveManageMaterialsOperation
{
    public required Guid Id { get; set; }
    public required int CaseId { get; set; }
    public required string OperationType { get; set; }
    public required string SourcePaths { get; set; }
    public string? DestinationPaths { get; set; }
    public string? UserName { get; set; }
    public DateTime CreatedAt { get; set; }
}
