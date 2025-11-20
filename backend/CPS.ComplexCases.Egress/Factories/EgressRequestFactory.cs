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

    if (arg.ViewFullDetails.HasValue && arg.ViewFullDetails.Value)
    {
      relativeUrl.Append("&view=full");
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

  public HttpRequestMessage GetWorkspacePermissionsByRoleIdRequest(GetWorkspacePermissionsByRoleIdArg arg, string token)
  {
    var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/workspaces/{arg.WorkspaceId}/roles/{arg.RoleId}?view=full");

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

    if (arg.Start.HasValue && arg.End.HasValue && arg.TotalSize.HasValue)
    {
      request.Content.Headers.ContentRange = new ContentRangeHeaderValue(arg.Start.Value, arg.End.Value, arg.TotalSize.Value);
    }

    AppendToken(request, token);

    return request;
  }

  public HttpRequestMessage CompleteUploadRequest(CompleteUploadArg arg, string token)
  {
    var completeData = new
    {
      done = true,
      md5_hash = arg.Md5Hash,
    };

    var request = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/workspaces/{arg.WorkspaceId}/uploads/{arg.UploadId}/")
    {
      Content = new StringContent(JsonSerializer.Serialize(completeData), Encoding.UTF8, "application/json")
    };

    AppendToken(request, token);

    return request;
  }

  public HttpRequestMessage CreateFolderRequest(CreateFolderArg arg, string token)
  {
    var folderData = new
    {
      folder_name = arg.FolderName,
    };

    var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/workspaces/{arg.WorkspaceId}/files?path={Uri.EscapeDataString(arg.Path ?? string.Empty)}")
    {
      Content = new StringContent(JsonSerializer.Serialize(folderData), Encoding.UTF8, "application/json")
    };

    AppendToken(request, token);

    return request;
  }

  public HttpRequestMessage DeleteFilesRequest(DeleteFilesArg arg, string token)
  {
    var fileData = new
    {
      file_ids = arg.FileIds
    };

    var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/workspaces/{arg.WorkspaceId}/files")
    {
      Content = new StringContent(JsonSerializer.Serialize(fileData), Encoding.UTF8, "application/json")
    };

    AppendToken(request, token);
    return request;
  }

  public HttpRequestMessage ListTemplatesRequest(PaginationArg arg, string token)
  {
    var relativeUrl = new StringBuilder($"/api/v1/templates?skip={arg.Skip}&limit={arg.Take}");

    var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl.ToString());

    AppendToken(request, token);

    return request;
  }

  public HttpRequestMessage CreateWorkspaceRequest(CreateEgressWorkspaceArg arg, string token)
  {

    var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/workspaces?view=full")
    {
      Content = new StringContent(JsonSerializer.Serialize(arg), Encoding.UTF8, "application/json")
    };

    AppendToken(request, token);

    return request;
  }

  public HttpRequestMessage GrantWorkspacePermissionRequest(GrantWorkspacePermissionArg arg, string token)
  {
    var permissionData = new
    {
      switch_ids = new[] { arg.Username },
      role_id = arg.RoleId
    };

    var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/workspaces/{arg.WorkspaceId}/users")
    {
      Content = new StringContent(JsonSerializer.Serialize(permissionData), Encoding.UTF8, "application/json")
    };

    AppendToken(request, token);

    return request;
  }

  public HttpRequestMessage ListWorkspaceRolesRequest(ListWorkspaceRolesArg arg, string token)
  {
    var relativeUrl = new StringBuilder($"/api/v1/workspaces/{arg.WorkspaceId}/roles?skip={arg.Skip}&limit={arg.Take}");

    var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl.ToString());

    AppendToken(request, token);

    return request;
  }

  private static void AppendToken(HttpRequestMessage request, string token)
  {
    var basicAuthValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicAuthValue);
  }
}