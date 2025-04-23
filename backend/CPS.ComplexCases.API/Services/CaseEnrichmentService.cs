using Microsoft.Extensions.Logging;
using CPS.ComplexCases.API.Domain.Response;
using CPS.ComplexCases.Data.Services;
using CPS.ComplexCases.DDEI.Models.Dto;
using CPS.ComplexCases.Egress.Models.Dto;
using CPS.ComplexCases.NetApp.Models.Dto;

namespace CPS.ComplexCases.API.Services;

public class CaseEnrichmentService : ICaseEnrichmentService
{
  private readonly ICaseMetadataService _caseMetadataService;
  private readonly ILogger<CaseEnrichmentService> _logger;

  public CaseEnrichmentService(
      ICaseMetadataService caseMetadataService,
      ILogger<CaseEnrichmentService> logger)
  {
    _caseMetadataService = caseMetadataService;
    _logger = logger;
  }

  public async Task<IEnumerable<CaseWithMetadataResponse>> EnrichCasesWithMetadataAsync(IEnumerable<CaseDto> cases)
  {
    if (!cases.Any())
    {
      return cases.Select(MapCaseToResponse);
    }

    _logger.LogInformation("Enriching {CaseCount} cases with metadata", cases.Count());

    try
    {
      var casesResponse = cases.Select(MapCaseToResponse).ToList();

      var caseIds = cases.Select(c => c.CaseId).ToList();
      var metadataLookup = (await _caseMetadataService.GetCaseMetadataForCaseIdsAsync(caseIds))
          .ToDictionary(m => m.CaseId);

      foreach (var caseResponse in casesResponse)
      {
        if (metadataLookup.TryGetValue(caseResponse.CaseId, out var metadata))
        {
          caseResponse.EgressWorkspaceId = metadata.EgressWorkspaceId;
          caseResponse.NetappFolderPath = metadata.NetappFolderPath;
        }
      }

      return casesResponse;
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Failed to retrieve or apply metadata for cases");

      return cases.Select(MapCaseToResponse);
    }
  }

  public async Task<ListWorkspacesResponse> EnrichEgressWorkspacesWithMetadataAsync(ListWorkspacesDto workspaces)
  {
    // Create base response without metadata first
    var response = CreateWorkspaceResponseBase(workspaces);

    if (!workspaces.Data.Any())
    {
      return response;
    }

    _logger.LogInformation("Enriching {WorkspaceCount} workspaces with metadata", workspaces.Data.Count());

    try
    {
      var workspaceIds = workspaces.Data.Select(w => w.Id).ToList();
      var metadata = await _caseMetadataService.GetCaseMetadataForEgressWorkspaceIdsAsync(workspaceIds);
      var metadataLookup = metadata
          .Where(m => m.EgressWorkspaceId != null)
          .ToDictionary(m => m.EgressWorkspaceId!);

      // Enrich data with metadata
      response.Data = workspaces.Data.Select(workspace => new ListWorkspaceDataResponse
      {
        Id = workspace.Id,
        Name = workspace.Name,
        DateCreated = workspace.DateCreated,
        CaseId = metadataLookup.TryGetValue(workspace.Id, out var caseMetadata) ? caseMetadata.CaseId : null
      }).ToList();

      return response;
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Failed to retrieve or apply metadata for workspaces");
      return response;
    }
  }

  public async Task<ListNetAppFoldersResponse> EnrichNetAppFoldersWithMetadataAsync(ListNetAppFoldersDto folders)
  {
    var response = CreateNetAppFoldersResponseBase(folders);

    if (!folders.Data.Any())
    {
      return response;
    }

    _logger.LogInformation("Enriching {NetAppFolderCount} workspaces with metadata", folders.Data.Count());

    try
    {
      var folderPaths = folders.Data.Where(d => d.Path != null)
                      .Select(d => $"{folders.BucketName}:{d.Path}")
                      .ToList();

      var metadata = await _caseMetadataService.GetCaseMetadataForNetAppFolderPathsAsync(folderPaths);
      var metadataLookup = metadata
          .Where(m => m.NetappFolderPath != null)
          .ToDictionary(m => m.NetappFolderPath!);

      // Enrich data with metadata
      response.Data = folderPaths.Select(folder => new ListNetAppFolderDataResponse
      {
        FolderPath = folder[(folder.LastIndexOf(':') + 1)..] ?? string.Empty,
        CaseId = metadataLookup.TryGetValue(folder, out var caseMetadata) ? caseMetadata.CaseId : null
      }).ToList();

      return response;
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Failed to retrieve or apply metadata for NetApp folders");
      return response;
    }
  }

  private static CaseWithMetadataResponse MapCaseToResponse(CaseDto caseDto)
  {
    return new CaseWithMetadataResponse
    {
      CaseId = caseDto.CaseId,
      Urn = caseDto.Urn,
      OperationName = caseDto.OperationName,
      LeadDefendantName = caseDto.LeadDefendantName,
      RegistrationDate = caseDto.RegistrationDate
    };
  }

  private static ListWorkspacesResponse CreateWorkspaceResponseBase(ListWorkspacesDto workspacesDto)
  {
    return new ListWorkspacesResponse
    {
      Pagination = new PaginationResponse
      {
        Count = workspacesDto.Pagination.Count,
        Take = workspacesDto.Pagination.Take,
        Skip = workspacesDto.Pagination.Skip,
        TotalResults = workspacesDto.Pagination.TotalResults
      },
      Data = workspacesDto.Data.Select(workspace => new ListWorkspaceDataResponse
      {
        Id = workspace.Id,
        Name = workspace.Name,
        DateCreated = workspace.DateCreated,
      }).ToList()
    };
  }

  private static ListNetAppFoldersResponse CreateNetAppFoldersResponseBase(ListNetAppFoldersDto foldersDto)
  {
    return new ListNetAppFoldersResponse
    {
      BucketName = foldersDto.BucketName,
      Pagination = new NetAppPaginationResponse
      {
        NextContinuationToken = foldersDto.DataInfo.NextContinuationToken,
        MaxKeys = foldersDto.DataInfo.MaxKeys,
      },
      Data = foldersDto.Data?.Where(folder => folder != null).Select(folder => new ListNetAppFolderDataResponse
      {
        FolderPath = folder.Path ?? string.Empty,
      }).ToList() ?? []
    };
  }
}