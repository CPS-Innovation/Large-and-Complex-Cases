
namespace CPS.ComplexCases.Egress.Models.Args;

public class ListWorkspaceMaterialArg : PaginationArg
{
  public required string WorkspaceId { get; set; }
  public string? FolderId { get; set; }
}
