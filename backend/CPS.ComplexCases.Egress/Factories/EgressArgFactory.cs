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

  public ListWorkspaceMaterialArg CreateListWorkspaceMaterialArg(string workspaceId, int skip, int take, string? folderId = null, bool? recurseSubFolders = null)
  {
    return new ListWorkspaceMaterialArg
    {
      WorkspaceId = workspaceId,
      Skip = skip,
      Take = take,
      FolderId = folderId,
      RecurseSubFolders = recurseSubFolders
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

  public PaginationArg CreatePaginationArg(int skip, int take)
  {
    return new PaginationArg
    {
      Skip = skip,
      Take = take
    };
  }

  public CreateEgressWorkspaceArg CreateEgressWorkspaceArg(string name, string? description, string templateId)
  {
    return new CreateEgressWorkspaceArg
    {
      Name = name,
      Description = description,
      TemplateId = templateId
    };
  }

  public GrantWorkspacePermissionArg CreateGrantWorkspacePermissionArg(string workspaceId, string email, string roleId)
  {
    return new GrantWorkspacePermissionArg
    {
      WorkspaceId = workspaceId,
      Username = email,
      RoleId = roleId
    };
  }
}