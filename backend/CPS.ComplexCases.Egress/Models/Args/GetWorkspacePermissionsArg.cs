
namespace CPS.ComplexCases.Egress.Models.Args;

public class GetWorkSpacePermissionArg
{
  public required string WorkspaceId { get; set; }
  public string? Email { get; set; }
}
