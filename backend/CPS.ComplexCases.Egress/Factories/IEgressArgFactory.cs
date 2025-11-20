using CPS.ComplexCases.Egress.Models.Args;

namespace CPS.ComplexCases.Egress.Factories;

public interface IEgressArgFactory
{
  ListEgressWorkspacesArg CreateListEgressWorkspacesArg(string? name, int skip, int take);
  ListWorkspaceMaterialArg CreateListWorkspaceMaterialArg(string caseId, int skip, int take, string? folderId = null, bool? recurseSubFolders = null);
  GetWorkspacePermissionArg CreateGetWorkspacePermissionArg(string workspaceId, string? email);
  PaginationArg CreatePaginationArg(int skip, int take);
  CreateEgressWorkspaceArg CreateEgressWorkspaceArg(string name, string? description, string templateId);
  GrantWorkspacePermissionArg CreateGrantWorkspacePermissionArg(string workspaceId, string email, string roleId);
}