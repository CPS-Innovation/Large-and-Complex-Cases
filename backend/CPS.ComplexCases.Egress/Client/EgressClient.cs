using CPS.ComplexCases.Common.Models.Domain.Dto;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.Egress.Models;
using CPS.ComplexCases.Egress.Models.Args;
using CPS.ComplexCases.Egress.Models.Dto;
using CPS.ComplexCases.Egress.Models.Response;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CPS.ComplexCases.Egress.Client;

public class EgressClient(
    ILogger<EgressClient> logger,
    IOptions<EgressOptions> egressOptions,
    HttpClient httpClient,
    IEgressRequestFactory egressRequestFactory) : BaseEgressClient(logger, egressOptions, httpClient, egressRequestFactory), IEgressClient
{
  public async Task<ListWorkspacesDto> ListWorkspacesAsync(ListEgressWorkspacesArg workspace, string email)
  {
    var token = await GetWorkspaceToken();
    var response = await SendRequestAsync<ListWorkspacesResponse>(_egressRequestFactory.ListWorkspacesRequest(workspace, token));

    var workspaces = response.Data
        .Select(data => new ListWorkspaceDataDto
        {
          Id = data.Id,
          Name = data.Name,
          DateCreated = data.DateCreated
        })
        .ToArray();

    var permissionTasks = workspaces.Select(async workspaceDto =>
    {
      var permissionsArg = new GetWorkspacePermissionArg
      {
        WorkspaceId = workspaceDto.Id,
        Email = email
      };

      var permissionsResponse = await SendRequestAsync<GetWorkspacePermissionsResponse>(_egressRequestFactory.GetWorkspacePermissionsRequest(permissionsArg, token));
      return (workspaceDto, permissionsResponse);
    });

    var permissionResults = await Task.WhenAll(permissionTasks);

    var filteredWorkspaces = permissionResults
        .Where(result => result.permissionsResponse.Data.Any(user => user.Email.Equals(email, StringComparison.CurrentCultureIgnoreCase)))
        .Select(result => result.workspaceDto)
        .ToArray();

    return new ListWorkspacesDto
    {
      Data = filteredWorkspaces,
      Pagination = new PaginationDto
      {
        Count = filteredWorkspaces.Length,
        Take = response.DataInfo.Limit,
        Skip = response.DataInfo.Skip,
        TotalResults = response.DataInfo.TotalResults
      }
    };
  }

  public async Task<ListCaseMaterialDto> ListCaseMaterialAsync(ListWorkspaceMaterialArg arg)
  {
    var token = await GetWorkspaceToken();

    if (arg.RecurseSubFolders == true)
    {
      var allFiles = await GetWorkspaceMaterials(arg, token);

      var pagedFiles = allFiles
          .Skip(arg.Skip)
          .Take(arg.Take)
          .ToList();

      return new ListCaseMaterialDto
      {
        Data = pagedFiles,
        Pagination = new PaginationDto
        {
          Count = pagedFiles.Count,
          Take = arg.Take,
          Skip = arg.Skip,
          TotalResults = allFiles.Count
        }
      };
    }
    else
    {
      var response = await SendRequestAsync<ListCaseMaterialResponse>(_egressRequestFactory.ListEgressMaterialRequest(arg, token));
      var materialsData = response.Data.Select(data => new ListCaseMaterialDataDto
      {
        Id = data.Id,
        Name = data.FileName,
        Path = data.Path,
        DateUpdated = data.DateUpdated,
        IsFolder = data.IsFolder,
        Version = data.Version,
        Filesize = data.FileSize
      });

      return new ListCaseMaterialDto
      {
        Data = materialsData,
        Pagination = new PaginationDto
        {
          Count = response.DataInfo.NumReturned,
          Take = response.DataInfo.Limit,
          Skip = response.DataInfo.Skip,
          TotalResults = response.DataInfo.TotalResults
        }
      };
    }
  }

  public async Task<bool> GetWorkspacePermission(GetWorkspacePermissionArg arg)
  {
    var token = await GetWorkspaceToken();
    var response = await SendRequestAsync<GetWorkspacePermissionsResponse>(_egressRequestFactory.GetWorkspacePermissionsRequest(arg, token));
    var user = response.Data.SingleOrDefault(user => user.Email.Equals(arg.Email, StringComparison.OrdinalIgnoreCase));

    if (user != null && arg.Permission != null)
    {
      var permissions = await GetPermissionsByRoleId(user.RoleId, arg.WorkspaceId, token);
      return permissions.Contains(arg.Permission);
    }
    else
    {
      // If no specific permission is requested, return if the user exists in the workspace
      return user != null;
    }
  }

  public async Task<ListTemplatesDto> ListTemplatesAsync(PaginationArg arg)
  {
    var token = await GetWorkspaceToken();
    var response = await SendRequestAsync<ListEgressTemplatesResponse>(_egressRequestFactory.ListTemplatesRequest(arg, token));

    var templates = response.Data
        .Select(data => new ListTemplateDataDto
        {
          Id = data.Id,
          Name = data.Name,
          Description = data.Description,
          Priority = data.Priority
        })
        .ToArray();

    return new ListTemplatesDto
    {
      Data = templates,
      Pagination = new PaginationDto
      {
        Count = response.DataInfo.NumReturned,
        Take = response.DataInfo.Limit,
        Skip = response.DataInfo.Skip,
        TotalResults = response.DataInfo.TotalResults
      }
    };
  }

  public async Task<CreateWorkspaceResponse> CreateWorkspaceAsync(CreateEgressWorkspaceArg arg)
  {
    var token = await GetWorkspaceToken();
    return await SendRequestAsync<CreateWorkspaceResponse>(_egressRequestFactory.CreateWorkspaceRequest(arg, token));
  }

  public async Task GrantWorkspacePermission(GrantWorkspacePermissionArg arg)
  {
    var token = await GetWorkspaceToken();
    await SendRequestAsync(_egressRequestFactory.GrantWorkspacePermissionRequest(arg, token));
  }

  public async Task<IEnumerable<ListWorkspaceRoleDto>> ListWorkspaceRolesAsync(string workspaceId)
  {
    var token = await GetWorkspaceToken();

    // paginated in Egress but doesn't need to be here (never more than a few roles)
    var arg = new ListWorkspaceRolesArg
    {
      WorkspaceId = workspaceId,
      Take = 100,
      Skip = 0
    };

    var response = await SendRequestAsync<ListWorkspaceRolesResponse>(_egressRequestFactory.ListWorkspaceRolesRequest(arg, token));

    return response.Data
        .Select(role => new ListWorkspaceRoleDto
        {
          RoleId = role.RoleId,
          RoleName = role.RoleName
        })
        .ToArray();

  }

  private async Task<IEnumerable<string>> GetPermissionsByRoleId(string roleId, string workspaceId, string token)
  {
    var arg = new GetWorkspacePermissionsByRoleIdArg
    {
      WorkspaceId = workspaceId,
      RoleId = roleId
    };

    var response = await SendRequestAsync<GetWorkspacePermissionsByRoleIdResponse>(_egressRequestFactory.GetWorkspacePermissionsByRoleIdRequest(arg, token));

    return response.FilePermissions != null
        ? response.FilePermissions
            .Distinct()
            .ToList()
        : [];
  }

  private async Task<List<ListCaseMaterialDataDto>> GetWorkspaceMaterials(ListWorkspaceMaterialArg currentArg, string token)
  {
    var response = await SendRequestAsync<ListCaseMaterialResponse>(_egressRequestFactory.ListEgressMaterialRequest(currentArg, token));
    var files = response.Data
        .Where(d => !d.IsFolder)
        .Select(d => new ListCaseMaterialDataDto
        {
          Id = d.Id,
          Name = d.FileName,
          Path = d.Path,
          DateUpdated = d.DateUpdated,
          IsFolder = d.IsFolder,
          Version = d.Version,
          Filesize = d.FileSize
        })
        .ToList();

    var folders = response.Data.Where(d => d.IsFolder).ToList();
    var subTasks = folders.Select(folder =>
    {
      var subArg = new ListWorkspaceMaterialArg
      {
        WorkspaceId = currentArg.WorkspaceId,
        FolderId = folder.Id,
        Take = currentArg.Take,
        Skip = 0,
        RecurseSubFolders = true
      };
      return GetWorkspaceMaterials(subArg, token);
    }).ToList();

    var subResults = await Task.WhenAll(subTasks);
    files.AddRange(subResults.SelectMany(x => x));
    return files;
  }
}
