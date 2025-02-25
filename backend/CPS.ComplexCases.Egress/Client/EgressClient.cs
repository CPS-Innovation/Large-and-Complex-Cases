using System.Text.Json;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.Egress.Factories;
using CPS.ComplexCases.Egress.Models;
using CPS.ComplexCases.Egress.Models.Response;
using CPS.ComplexCases.Egress.Models.Dto;
using CPS.ComplexCases.Egress.Models.Args;
using Microsoft.Extensions.Options;

namespace CPS.ComplexCases.Egress.Client;

public class EgressClient(ILogger<EgressClient> logger, IOptions<EgressOptions> egressOptions, HttpClient httpClient, IEgressRequestFactory egressRequestFactory) : IEgressClient
{
  private readonly ILogger<EgressClient> _logger = logger;
  private readonly EgressOptions _egressOptions = egressOptions.Value;
  private readonly HttpClient _httpClient = httpClient;
  private readonly IEgressRequestFactory _egressRequestFactory = egressRequestFactory;

  public async Task<IEnumerable<FindWorkspaceDto>> FindWorkspace(FindWorkspaceArg workspace, string email)
  {
    var token = await GetWorkspaceToken();
    var response = await SendRequestAsync<FindWorkspaceResponse>(_egressRequestFactory.FindWorkspaceRequest(workspace, token));

    var workspaces = response.Data
        .Select(data => new FindWorkspaceDto
        {
          Id = data.Id,
          EgressLink = $"{_egressOptions.Url}w/edit/{data.Id}",
          Name = data.Name
        })
        .ToList();

    var permissionTasks = workspaces.Select(async workspaceDto =>
    {
      var permissionsArg = new GetWorkSpacePermissionArg
      {
        WorkspaceId = workspaceDto.Id,
        Email = email
      };

      var permissionsResponse = await SendRequestAsync<GetWorkspacePermissionsResponse>(_egressRequestFactory.GetWorkspacePermissionsRequest(permissionsArg, token));
      return (workspaceDto, permissionsResponse);
    });

    var permissionResults = await Task.WhenAll(permissionTasks);

    var filteredWorkspaces = permissionResults
        .Where(result => result.permissionsResponse.Data.Any(user => user.Email == email))
        .Select(result => result.workspaceDto);

    return filteredWorkspaces;
  }

  public async Task<GetCaseMaterialDto> GetCaseMaterial(GetCaseMaterialArg arg)
  {
    var token = await GetWorkspaceToken();
    var response = await SendRequestAsync<GetCaseMaterialResponse>(_egressRequestFactory.GetCaseMaterialRequest(arg, token));

    var materialsData = response.Data.Select(data => new GetCaseMaterialDataDto
    {
      Id = data.Id,
      FileName = data.FileName,
      Path = data.Path,
      DateUpdated = data.DateUpdated,
      IsFolder = data.IsFolder,
      Version = data.Version
    });

    return new GetCaseMaterialDto
    {
      PerPage = response.Pagination.PerPage,
      TotalPages = response.Pagination.TotalPages,
      TotalResults = response.Pagination.TotalResults,
      CurrentPage = response.Pagination.CurrentPage,
      Data = materialsData
    };
  }

  public async Task<Stream> GetCaseDocument(GetCaseDocumentArg arg)
  {
    var token = await GetWorkspaceToken();
    var response = await SendRequestAsync(_egressRequestFactory.GetCaseDocumentRequest(arg, token));
    return await response.Content.ReadAsStreamAsync();
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
    catch (HttpRequestException ex)
    {
      _logger.LogError(ex, "Error sending request to egress service");
      throw;
    }
  }
}