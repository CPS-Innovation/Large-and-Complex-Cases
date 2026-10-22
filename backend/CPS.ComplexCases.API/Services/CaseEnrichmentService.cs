using CPS.ComplexCases.API.Domain.Response;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.DDEI.Client;
using CPS.ComplexCases.DDEI.Factories;
using CPS.ComplexCases.DDEI.Models.Dto;
using CPS.ComplexCases.Egress.Models.Dto;
using CPS.ComplexCases.NetApp.Models.Dto;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.API.Services;

public class CaseEnrichmentService : ICaseEnrichmentService
{
    private readonly ICaseMetadataService _caseMetadataService;
    private readonly IDdeiClient _ddeiClient;
    private readonly IDdeiArgFactory _ddeiArgFactory;
    private readonly ILogger<CaseEnrichmentService> _logger;

    public CaseEnrichmentService(
        ICaseMetadataService caseMetadataService,
        IDdeiClient ddeiClient,
        IDdeiArgFactory ddeiArgFactory,
        ILogger<CaseEnrichmentService> logger)
    {
        _caseMetadataService = caseMetadataService;
        _ddeiClient = ddeiClient;
        _ddeiArgFactory = ddeiArgFactory;
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

    public async Task<ListWorkspacesResponse> EnrichEgressWorkspacesWithMetadataAsync(ListWorkspacesDto workspaces, string cmsAuthValues, Guid correlationId)
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
            var metadataLookup = BuildMetadataLookup(metadata, m => m.EgressWorkspaceId, "Egress workspace");

            var connectedCaseIds = metadataLookup.Values.Select(m => m.CaseId).Distinct();
            var connectedCases = await GetConnectedCasesAsync(connectedCaseIds, cmsAuthValues, correlationId);

            // Enrich data with metadata
            response.Data = workspaces.Data.Select(workspace =>
            {
                int? caseId = metadataLookup.TryGetValue(workspace.Id, out var caseMetadata) ? caseMetadata.CaseId : null;
                var connectedCase = caseId.HasValue ? connectedCases.GetValueOrDefault(caseId.Value) : null;

                return new ListWorkspaceDataResponse
                {
                    Id = workspace.Id,
                    Name = workspace.Name,
                    DateCreated = workspace.DateCreated,
                    CaseId = caseId,
                    ConnectedCaseUrn = connectedCase?.Urn,
                    ConnectedCaseName = connectedCase == null ? null : ResolveCaseName(connectedCase)
                };
            }).ToList();

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve or apply metadata for workspaces");
            return response;
        }
    }

    public async Task<ListNetAppObjectsResponse> EnrichNetAppFoldersWithMetadataAsync(ListNetAppObjectsDto folders)
    {
        var response = CreateNetAppFoldersResponseBase(folders);

        if (!folders.Data.FolderData.Any())
        {
            return response;
        }

        _logger.LogInformation("Enriching {NetAppFolderCount} workspaces with metadata", folders.Data.FolderData.Count());

        try
        {
            var folderPaths = folders.Data.FolderData
                .Where(d => d.Path != null)
                .Select(d => d.Path!)
                .ToList();

            // Look up plain paths (as stored on connect) and bucket:path variants for any legacy rows
            var lookupPaths = folderPaths
                .SelectMany(path => new[] { path, $"{folders.Data.BucketName}:{path}" })
                .Distinct()
                .ToList();

            var metadata = await _caseMetadataService.GetCaseMetadataForNetAppFolderPathsAsync(lookupPaths);
            var metadataLookup = BuildMetadataLookup(metadata, m => m.NetappFolderPath, "NetApp folder path");

            // Enrich data with metadata
            response.Data = new ListNetAppObjectsDataResponse
            {
                Folders = folderPaths.Select(folderPath => new ListNetAppFoldersDataResponse
                {
                    FolderPath = folderPath,
                    CaseId = ResolveNetAppFolderCaseId(folderPath, folders.Data.BucketName, metadataLookup)
                }).ToList(),
                Files = new List<ListNetAppFilesDataResponse>(),
                RootPath = folders.Data.RootPath
            };

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve or apply metadata for NetApp folders");
            return response;
        }
    }

    // Only case_id is unique in case_metadata, so two cases can share a workspace or folder path.
    // A duplicate must not cost the caller every result on the page, so pick the lowest case id.
    private Dictionary<string, CaseMetadata> BuildMetadataLookup(
        IEnumerable<CaseMetadata> metadata,
        Func<CaseMetadata, string?> keySelector,
        string keyDescription)
    {
        var lookup = new Dictionary<string, CaseMetadata>();

        foreach (var group in metadata.Where(m => keySelector(m) != null).GroupBy(m => keySelector(m)!))
        {
            var orderedByCaseId = group.OrderBy(m => m.CaseId).ToList();

            if (orderedByCaseId.Count > 1)
            {
                _logger.LogWarning(
                    "{KeyDescription} {Key} is linked to {CaseCount} cases ({CaseIds}). Using case {CaseId}.",
                    keyDescription,
                    group.Key,
                    orderedByCaseId.Count,
                    string.Join(", ", orderedByCaseId.Select(m => m.CaseId)),
                    orderedByCaseId[0].CaseId);
            }

            lookup[group.Key] = orderedByCaseId[0];
        }

        return lookup;
    }

    private async Task<Dictionary<int, CaseDto>> GetConnectedCasesAsync(
        IEnumerable<int> caseIds,
        string cmsAuthValues,
        Guid correlationId)
    {
        var lookups = await Task.WhenAll(caseIds.Select(async caseId =>
            (CaseId: caseId, Case: await TryGetCaseAsync(caseId, cmsAuthValues, correlationId))));

        return lookups
            .Where(l => l.Case != null)
            .ToDictionary(l => l.CaseId, l => l.Case!);
    }

    // A failed DDEI lookup for one case must not cost the caller the rest of the page.
    private async Task<CaseDto?> TryGetCaseAsync(int caseId, string cmsAuthValues, Guid correlationId)
    {
        try
        {
            var arg = _ddeiArgFactory.CreateCaseArg(cmsAuthValues, correlationId, caseId);
            var caseDto = await _ddeiClient.GetCaseAsync(arg);

            if (caseDto == null)
            {
                _logger.LogWarning("DDEI returned no case details for connected case {CaseId}", caseId);
            }

            return caseDto;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve case details from DDEI for connected case {CaseId}", caseId);
            return null;
        }
    }

    private static string? ResolveCaseName(CaseDto caseDto) =>
        string.IsNullOrWhiteSpace(caseDto.OperationName) ? caseDto.LeadDefendantName : caseDto.OperationName;

    private static int? ResolveNetAppFolderCaseId(
        string folderPath,
        string bucketName,
        IReadOnlyDictionary<string, CaseMetadata> metadataLookup)
    {
        if (metadataLookup.TryGetValue(folderPath, out var caseMetadata))
        {
            return caseMetadata.CaseId;
        }

        if (metadataLookup.TryGetValue($"{bucketName}:{folderPath}", out caseMetadata))
        {
            return caseMetadata.CaseId;
        }

        return null;
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

    private static ListNetAppObjectsResponse CreateNetAppFoldersResponseBase(ListNetAppObjectsDto foldersDto)
    {
        return new ListNetAppObjectsResponse
        {

            Pagination = new NetAppPaginationResponse
            {
                NextContinuationToken = foldersDto.Pagination.NextContinuationToken,
                MaxKeys = foldersDto.Pagination.MaxKeys,
            },
            Data = new ListNetAppObjectsDataResponse
            {
                Folders = foldersDto.Data.FolderData.Select(folder => new ListNetAppFoldersDataResponse
                {
                    FolderPath = folder.Path ?? string.Empty,
                    CaseId = null
                }).ToList(),
                Files = new List<ListNetAppFilesDataResponse>(),
                RootPath = foldersDto.Data.RootPath ?? string.Empty
            }
        };
    }
}
