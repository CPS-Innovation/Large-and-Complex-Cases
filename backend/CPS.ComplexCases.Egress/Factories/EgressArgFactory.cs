using CPS.ComplexCases.Egress.Models.Args;

namespace CPS.ComplexCases.Egress.Factories;

public class EgressArgFactory : IEgressArgFactory
{
  public ListEgressWorkspacesArg CreateListEgressWorkspacesArg(string? name, int skip, int take)
  {
    return new ListEgressWorkspacesArg
    {
      Name = name,
      Skip = skip,
      Take = take
    };
  }

  public ListWorkspaceMaterialArg CreateListWorkspaceMaterialArg(string workspaceId, int skip, int take, string? folderId)
  {
    return new ListWorkspaceMaterialArg
    {
      WorkspaceId = workspaceId,
      Skip = skip,
      Take = take,
      FolderId = folderId
    };
  }

  public GetWorkspacePermissionArg CreateGetWorkspacePermissionArg(string workspaceId, string? email)
  {
    return new GetWorkspacePermissionArg
    {
      WorkspaceId = workspaceId,
      Email = email
    };
  }
}