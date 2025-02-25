
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

  public HttpRequestMessage FindWorkspaceRequest(FindWorkspaceArg workspace, string token)
  {
    var relativeUrl = new StringBuilder($"/api/v1/workspaces");

    if (!string.IsNullOrEmpty(workspace.Name))
    {
      relativeUrl.Append($"?name={workspace.Name}");
    }

    var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl.ToString());

    AppendToken(request, token);

    return request;
  }

  public HttpRequestMessage GetCaseMaterialRequest(GetCaseMaterialArg arg, string token)
  {
    var relativeUrl = new StringBuilder($"/api/v1/workspaces/{arg.CaseId}/files");
    var query = $"?view=full&page={arg.Page}&count={arg.Count}";

    if (!string.IsNullOrEmpty(arg.FolderId))
    {
      query += $"&folder={arg.FolderId}";
    }

    relativeUrl.Append(query);

    var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl.ToString());

    AppendToken(request, token);

    return request;
  }

  public HttpRequestMessage GetWorkspacePermissionsRequest(GetWorkSpacePermissionArg arg, string token)
  {
    var relativeUrl = new StringBuilder($"/api/v1/workspaces/{arg.WorkspaceId}/users");

    if (!string.IsNullOrEmpty(arg.Email))
    {
      relativeUrl.Append($"?switch_id={arg.Email}");
    }

    var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl.ToString());


    AppendToken(request, token);

    return request;
  }

  public HttpRequestMessage GetCaseDocumentRequest(GetCaseDocumentArg arg, string token)
  {
    var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/workspaces/{arg.CaseId}/files/{arg.FileId}");

    AppendToken(request, token);

    return request;
  }

  private static void AppendToken(HttpRequestMessage request, string token)
  {
    var basicAuthValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicAuthValue);
  }
}