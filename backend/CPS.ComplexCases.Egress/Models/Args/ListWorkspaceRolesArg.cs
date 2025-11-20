
namespace CPS.ComplexCases.Egress.Models.Args;

public class ListWorkspaceRolesArg : PaginationArg
{
    public required string WorkspaceId { get; set; }
}
