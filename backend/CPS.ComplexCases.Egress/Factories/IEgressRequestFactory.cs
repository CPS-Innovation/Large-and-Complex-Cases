using CPS.ComplexCases.Egress.Models.Args;

namespace CPS.ComplexCases.Egress.Factories;

public interface IEgressRequestFactory
{
  HttpRequestMessage GetWorkspaceTokenRequest(string serviceAccountUsername, string serviceAccountPassword);
  HttpRequestMessage FindWorkspaceRequest(FindWorkspaceArg workspace, string token);
  HttpRequestMessage GetWorkspaceMaterialRequest(GetWorkspaceMaterialArg arg, string token);
  HttpRequestMessage GetWorkspacePermissionsRequest(GetWorkSpacePermissionArg arg, string token);
  HttpRequestMessage GetWorkspaceDocumentRequest(GetWorkspaceDocumentArg arg, string token);
}
