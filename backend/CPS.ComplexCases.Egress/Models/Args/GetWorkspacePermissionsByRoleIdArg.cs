namespace CPS.ComplexCases.Egress.Models.Args;

public class GetWorkspacePermissionsByRoleIdArg
{
    public required string WorkspaceId { get; set; }
    public required string RoleId { get; set; }
}