using CPS.ComplexCases.Egress.Models.Args;

namespace CPS.ComplexCases.Egress.Factories;

public class EgressArgFactory : IEgressArgFactory
{
  public FindWorkspaceArg CreateFindWorkspaceArg(string? name)
  {
    return new FindWorkspaceArg
    {
      Name = name
    };
  }

  public GetWorkspaceMaterialArg CreateGetWorkspaceMaterialArg(string workspaceId, int page, int count, string? folderId)
  {
    return new GetWorkspaceMaterialArg
    {
      WorkspaceId = workspaceId,
      Page = page,
      Count = count,
      FolderId = folderId
    };
  }
}