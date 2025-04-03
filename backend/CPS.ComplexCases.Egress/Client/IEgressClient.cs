
using CPS.ComplexCases.Egress.Models.Args;
using CPS.ComplexCases.Egress.Models.Dto;

namespace CPS.ComplexCases.Egress.Client;

public interface IEgressClient
{
  Task<ListWorkspacesDto> ListWorkspacesAsync(ListEgressWorkspacesArg arg, string email);
  Task<bool> GetWorkspacePermission(GetWorkSpacePermissionArg arg);
  Task<GetCaseMaterialDto> GetCaseMaterial(GetWorkspaceMaterialArg arg);
  Task<Stream> GetCaseDocument(GetWorkspaceDocumentArg arg);
}
