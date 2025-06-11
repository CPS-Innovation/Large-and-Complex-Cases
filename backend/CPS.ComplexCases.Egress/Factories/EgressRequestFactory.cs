using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
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

    if (!string.IsNullOrEmpty(arg.Path))
    {
      relativeUrl.Append($"&path={arg.Path}");
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

  public HttpRequestMessage CreateUploadRequest(CreateUploadArg arg, string token)
  {
    var uploadData = new
    {
      filename = arg.FileName,
      filesize = arg.FileSize,
      folder_path = arg.FolderPath,
    };

    var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/workspaces/{arg.WorkspaceId}/uploads")
    {
      Content = new StringContent(JsonSerializer.Serialize(uploadData), Encoding.UTF8, "application/json")
    };

    AppendToken(request, token);

    return request;
  }

  public HttpRequestMessage UploadChunkRequest(UploadChunkArg arg, string token)
  {
    var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/v1/workspaces/{arg.WorkspaceId}/uploads/{arg.UploadId}/");

    var content = new MultipartFormDataContent();
    var fileContent = new ByteArrayContent(arg.ChunkData);
    content.Add(fileContent, "file_content");
    request.Content = content;

    if (!string.IsNullOrEmpty(arg.ContentRange))
    {
      // NOTE: Egress API expects Content-Range header in the format "bytes start-end/total"
      request.Headers.TryAddWithoutValidation("Content-Range", arg.ContentRange);
    }

    AppendToken(request, token);

    return request;
  }

  public HttpRequestMessage CompleteUploadRequest(CompleteUploadArg arg, string token)
  {
    var completeData = new
    {
      md5_hash = arg.Md5Hash,
      done = true
    };

    var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/workspaces/{arg.WorkspaceId}/uploads/{arg.UploadId}/")
    {
      Content = new StringContent(JsonSerializer.Serialize(completeData), Encoding.UTF8, "application/json")
    };

    AppendToken(request, token);

    return request;
  }

  private static void AppendToken(HttpRequestMessage request, string token)
  {
    var basicAuthValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicAuthValue);
  }
}