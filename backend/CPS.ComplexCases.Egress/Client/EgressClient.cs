using System.Net;
using System.Text.Json;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.Egress.Models;
using CPS.ComplexCases.Egress.Models.Args;
using CPS.ComplexCases.Egress.Models.Dto;
using CPS.ComplexCases.Egress.Models.Response;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CPS.ComplexCases.Egress.Client;

public class EgressClient(ILogger<EgressClient> logger, IOptions<EgressOptions> egressOptions, HttpClient httpClient, IEgressRequestFactory egressRequestFactory) : IEgressClient
{
  private readonly ILogger<EgressClient> _logger = logger;
  private readonly EgressOptions _egressOptions = egressOptions.Value;
  private readonly HttpClient _httpClient = httpClient;
  private readonly IEgressRequestFactory _egressRequestFactory = egressRequestFactory;

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
    var response = await SendRequestAsync<ListCaseMaterialResponse>(_egressRequestFactory.ListEgressMaterialRequest(arg, token));

    var materialsData = response.Data.Select(data => new ListCaseMaterialDataDto
    {
      Id = data.Id,
      Name = data.FileName,
      Path = data.Path,
      DateUpdated = data.DateUpdated,
      IsFolder = data.IsFolder,
      Version = data.Version
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

  public async Task<Stream> GetCaseDocument(GetWorkspaceDocumentArg arg)
  {
    var token = await GetWorkspaceToken();
    var response = await SendRequestAsync(_egressRequestFactory.GetWorkspaceDocumentRequest(arg, token));
    return await response.Content.ReadAsStreamAsync();
  }

  public async Task<bool> GetWorkspacePermission(GetWorkspacePermissionArg arg)
  {
    var token = await GetWorkspaceToken();
    var response = await SendRequestAsync<GetWorkspacePermissionsResponse>(_egressRequestFactory.GetWorkspacePermissionsRequest(arg, token));
    return response.Data.Any(user => user.Email.Equals(arg.Email, StringComparison.CurrentCultureIgnoreCase));
  }

  private async Task<string> GetWorkspaceToken()
  {
    var response = await SendRequestAsync<GetWorkspaceTokenResponse>(_egressRequestFactory.GetWorkspaceTokenRequest(_egressOptions.Username, _egressOptions.Password));
    return response.Token;
  }

  private async Task<T> SendRequestAsync<T>(HttpRequestMessage request)
  {
    using var response = await SendRequestAsync(request);
    var responseContent = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<T>(responseContent) ?? throw new InvalidOperationException("Deserialization returned null.");
    return result;
  }

  private async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request)
  {
    var response = await _httpClient.SendAsync(request);
    try
    {
      response.EnsureSuccessStatusCode();
      return response;
    }
    catch (HttpRequestException ex) when (response.StatusCode == HttpStatusCode.NotFound)
    {
      _logger.LogWarning(ex, "Workspace not found. Check the workspace ID.");
      throw;

    }
    catch (HttpRequestException ex)
    {
      _logger.LogError(ex, "Error sending request to egress service");
      throw;
    }
  }
}