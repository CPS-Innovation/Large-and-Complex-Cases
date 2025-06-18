
namespace CPS.ComplexCases.Data.Entities;

public class CaseMetadata
{
  public required int CaseId { get; set; }
  public string? EgressWorkspaceId { get; set; }
  public string? NetappFolderPath { get; set; }
  public Guid? ActiveTransferId { get; set; }
}
