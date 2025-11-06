
using CPS.ComplexCases.Egress.Models.Args;
using CPS.ComplexCases.Egress.Models.Dto;
using CPS.ComplexCases.Egress.Models.Response;

namespace CPS.ComplexCases.Egress.Client;

public interface IEgressClient
{
  Task<ListWorkspacesDto> ListWorkspacesAsync(ListEgressWorkspacesArg arg, string email);
  Task<bool> GetWorkspacePermission(GetWorkspacePermissionArg arg);
  Task<ListCaseMaterialDto> ListCaseMaterialAsync(ListWorkspaceMaterialArg arg);
  Task<ListTemplatesDto> ListTemplatesAsync(PaginationArg arg);
  Task<CreateWorkspaceResponse> CreateWorkspaceAsync(CreateEgressWorkspaceArg arg);
  Task GrantWorkspacePermission(GrantWorkspacePermissionArg arg);
  Task<IEnumerable<ListWorkspaceRoleDto>> ListWorkspaceRolesAsync(string workspaceId);
}
