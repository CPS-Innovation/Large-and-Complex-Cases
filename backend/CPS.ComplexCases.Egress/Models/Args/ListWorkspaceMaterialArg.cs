
namespace CPS.ComplexCases.Egress.Models.Args;

public class ListWorkspaceMaterialArg : PaginationArg
{
  public required string WorkspaceId { get; set; }
  public string? FolderId { get; set; }
  public bool? RecurseSubFolders { get; set; } = false;
  public string? Path { get; set; }
  public bool? ViewFullDetails { get; set; } = false;
}
