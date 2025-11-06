namespace CPS.ComplexCases.Egress.Models.Args;

public class GrantWorkspacePermissionArg
{
    public required string Username { get; set; }
    public required string RoleId { get; set; }
    public required string WorkspaceId { get; set; }
}
