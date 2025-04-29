
using System.Net.Http.Headers;
using System.Text;
using CPS.ComplexCases.Egress.Models.Args;

namespace CPS.ComplexCases.Egress.Factories;

public class EgressRequestFactory : IEgressRequestFactory
{
  public HttpRequestMessage GetWorkspaceTokenRequest(string serviceAccountUsername, string serviceAccountPassword)
  {
    var request = new HttpRequestMessage(HttpMethod.Get, "api/v1/user/auth/");
    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{serviceAccountUsername}:{serviceAccountPassword}")));

    return request;
  }

  public HttpRequestMessage ListWorkspacesRequest(ListEgressWorkspacesArg arg, string token)
  {
    var relativeUrl = new StringBuilder($"/api/v1/workspaces?view=full&skip={arg.Skip}&limit={arg.Take}");

    if (!string.IsNullOrEmpty(arg.Name))
    {
      relativeUrl.Append($"&name={arg.Name}");
    }

    var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl.ToString());

    AppendToken(request, token);

    return request;
  }

  public HttpRequestMessage ListEgressMaterialRequest(ListWorkspaceMaterialArg arg, string token)
  {
    var relativeUrl = new StringBuilder($"/api/v1/workspaces/{arg.WorkspaceId}/files?view=full&skip={arg.Skip}&limit={arg.Take}");

    if (!string.IsNullOrEmpty(arg.FolderId))
    {
      relativeUrl.Append($"&folder={arg.FolderId}");
    }

    var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl.ToString());

    AppendToken(request, token);

    return request;
  }

  public HttpRequestMessage GetWorkspacePermissionsRequest(GetWorkspacePermissionArg arg, string token)
  {
    // pagination is not being used internally here because we always filter on the users email (switch_id in Egress)
    var relativeUrl = new StringBuilder($"/api/v1/workspaces/{arg.WorkspaceId}/users?skip=0&limit=100");

    if (!string.IsNullOrEmpty(arg.Email))
    {
      relativeUrl.Append($"&switch_id={arg.Email}");
    }

    var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl.ToString());


    AppendToken(request, token);

    return request;
  }

  public HttpRequestMessage GetWorkspaceDocumentRequest(GetWorkspaceDocumentArg arg, string token)
  {
    var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/workspaces/{arg.WorkspaceId}/files/{arg.FileId}");

    AppendToken(request, token);

    return request;
  }

  private static void AppendToken(HttpRequestMessage request, string token)
  {
    var basicAuthValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicAuthValue);
  }
}