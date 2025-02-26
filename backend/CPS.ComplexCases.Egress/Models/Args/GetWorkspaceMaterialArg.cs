
namespace CPS.ComplexCases.Egress.Models.Args;

public class GetWorkspaceMaterialArg : PaginationArg
{
  public required string WorkspaceId { get; set; }
  public string? FolderId { get; set; }
}
