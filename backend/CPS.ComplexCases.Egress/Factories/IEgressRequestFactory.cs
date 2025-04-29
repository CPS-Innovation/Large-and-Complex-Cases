using CPS.ComplexCases.Egress.Models.Args;

namespace CPS.ComplexCases.Egress.Factories;

public interface IEgressRequestFactory
{
  HttpRequestMessage GetWorkspaceTokenRequest(string serviceAccountUsername, string serviceAccountPassword);
  HttpRequestMessage ListWorkspacesRequest(ListEgressWorkspacesArg arg, string token);
  HttpRequestMessage ListEgressMaterialRequest(ListWorkspaceMaterialArg arg, string token);
  HttpRequestMessage GetWorkspacePermissionsRequest(GetWorkspacePermissionArg arg, string token);
  HttpRequestMessage GetWorkspaceDocumentRequest(GetWorkspaceDocumentArg arg, string token);
}
