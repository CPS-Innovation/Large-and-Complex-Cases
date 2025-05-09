
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.Data.Models.Requests;
using CPS.ComplexCases.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.Data.Services;

public class CaseMetadataService : ICaseMetadataService
{
  private readonly ICaseMetadataRepository _caseMetadataRepository;
  private readonly ILogger<CaseMetadataService> _logger;

  public CaseMetadataService(ICaseMetadataRepository caseMetadataRepository, ILogger<CaseMetadataService> logger)
  {
    _caseMetadataRepository = caseMetadataRepository;
    _logger = logger;
  }

  public async Task CreateEgressConnectionAsync(CreateEgressConnectionDto createEgressConnectionDto)
  {
    _logger.LogInformation("Creating egress connection for case {CaseId}", createEgressConnectionDto.CaseId);
    try
    {
      var existingMetadata = await _caseMetadataRepository.GetByCaseIdAsync(createEgressConnectionDto.CaseId);

      if (existingMetadata != null)
      {
        existingMetadata.EgressWorkspaceId = createEgressConnectionDto.EgressWorkspaceId;
        await _caseMetadataRepository.UpdateAsync(existingMetadata);
        return;
      }
      else
      {
        var newMetadata = new CaseMetadata
        {
          CaseId = createEgressConnectionDto.CaseId,
          EgressWorkspaceId = createEgressConnectionDto.EgressWorkspaceId
        };
        await _caseMetadataRepository.AddAsync(newMetadata);
        return;
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error creating egress connection for case {CaseId}", createEgressConnectionDto.CaseId);
      throw;
    }
  }

  public async Task CreateNetAppConnectionAsync(CreateNetAppConnectionDto createNetAppConnectionDto)
  {
    _logger.LogInformation("Creating egress connection for case {CaseId}", createNetAppConnectionDto.CaseId);
    try
    {
      var netAppFolderPath = $"{createNetAppConnectionDto.OperationName}:{createNetAppConnectionDto.NetAppFolderPath}";
      var existingMetadata = await _caseMetadataRepository.GetByCaseIdAsync(createNetAppConnectionDto.CaseId);

      if (existingMetadata != null)
      {
        existingMetadata.NetappFolderPath = netAppFolderPath;
        await _caseMetadataRepository.UpdateAsync(existingMetadata);
        return;
      }
      else
      {
        var newMetadata = new CaseMetadata
        {
          CaseId = createNetAppConnectionDto.CaseId,
          NetappFolderPath = netAppFolderPath
        };
        await _caseMetadataRepository.AddAsync(newMetadata);
        return;
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error creating NetApp connection for case {CaseId}", createNetAppConnectionDto.CaseId);
      throw;
    }
  }

  public async Task<IEnumerable<CaseMetadata>> GetCaseMetadataForCaseIdsAsync(IEnumerable<int> caseIds)
  {
    _logger.LogInformation("Retrieving metadata for {Count} cases", caseIds.Count());
    try
    {
      return await _caseMetadataRepository.GetByCaseIdsAsync(caseIds);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving metadata for multiple cases");
      throw;
    }
  }
  public async Task<CaseMetadata?> GetCaseMetadataForCaseIdAsync(int caseId)
  {
    _logger.LogInformation("Retrieving metadata for case {CaseId}", caseId);
    try
    {
      return await _caseMetadataRepository.GetByCaseIdAsync(caseId);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving metadata for case {CaseId}", caseId);
      throw;
    }
  }
  public async Task<IEnumerable<CaseMetadata>> GetCaseMetadataForEgressWorkspaceIdsAsync(IEnumerable<string> egressWorkspaceIds)
  {
    _logger.LogInformation("Retrieving metadata for {Count} egress workspaces", egressWorkspaceIds.Count());
    try
    {
      return await _caseMetadataRepository.GetByEgressWorkspaceIdsAsync(egressWorkspaceIds);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving metadata for multiple egress workspaces");
      throw;
    }
  }

  public Task<IEnumerable<CaseMetadata>> GetCaseMetadataForNetAppFolderPathsAsync(IEnumerable<string> netAppFolderPaths)
  {
    _logger.LogInformation("Retrieving metadata for {Count} NetApp folder paths", netAppFolderPaths.Count());
    try
    {
      return _caseMetadataRepository.GetByNetAppFolderPathsAsync(netAppFolderPaths);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving metadata for multiple NetApp folder paths");
      throw;
    }
  }
}