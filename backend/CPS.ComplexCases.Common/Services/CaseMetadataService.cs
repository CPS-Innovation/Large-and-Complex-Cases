using CPS.ComplexCases.Common.Enums;
using CPS.ComplexCases.Common.Models.Results;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.Data.Models.Requests;
using CPS.ComplexCases.Data.Repositories;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.Common.Services;

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
      var existingMetadata = await _caseMetadataRepository.GetByCaseIdAsync(createNetAppConnectionDto.CaseId);

      if (existingMetadata != null)
      {
        existingMetadata.NetappFolderPath = createNetAppConnectionDto.NetAppFolderPath;
        await _caseMetadataRepository.UpdateAsync(existingMetadata);
        return;
      }
      else
      {
        var newMetadata = new CaseMetadata
        {
          CaseId = createNetAppConnectionDto.CaseId,
          NetappFolderPath = createNetAppConnectionDto.NetAppFolderPath
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

  public async Task UpdateActiveTransferIdAsync(int caseId, Guid? activeTransferId)
  {
    _logger.LogInformation("Updating active transfer ID for case {CaseId}", caseId);
    try
    {
      var existingMetadata = await _caseMetadataRepository.GetByCaseIdAsync(caseId);

      if (existingMetadata != null)
      {
        existingMetadata.ActiveTransferId = activeTransferId;
        await _caseMetadataRepository.UpdateAsync(existingMetadata);
      }
      else
      {
        _logger.LogWarning("No metadata found for case {CaseId} to update active transfer ID", caseId);
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error updating active transfer ID for case {CaseId}", caseId);
      throw;
    }
  }

  public async Task<bool> ClearActiveTransferIdAsync(Guid transferId)
  {
    _logger.LogInformation("Clearing active transfer ID for transfer {TransferId}", transferId);
    try
    {
      var existingMetadata = await _caseMetadataRepository.GetByActiveTransferIdAsync(transferId);

      if (existingMetadata != null)
      {
        existingMetadata.ActiveTransferId = null;
        await _caseMetadataRepository.UpdateAsync(existingMetadata);
        return true;
      }
      else
      {
        _logger.LogWarning("No metadata found for transfer {TransferId} to clear active transfer ID", transferId);
        return false;
      }
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error clearing active transfer ID for transfer {TransferId}", transferId);
      throw;
    }
  }

  public Task<ClearFolderPathResult> ClearNetAppFolderPathAsync(int caseId) =>
    ClearConnectionAsync(
      caseId,
      logContext: "NetApp folder path",
      getValue: m => m.NetappFolderPath,
      clearValue: m => m.NetappFolderPath = null,
      missingValueState: CaseMetadataState.NetAppFolderPathIsNull
    );

  public Task<ClearFolderPathResult> ClearEgressConnectionAsync(int caseId) =>
    ClearConnectionAsync(
      caseId,
      logContext: "Egress workspace connection",
      getValue: m => m.EgressWorkspaceId,
      clearValue: m => m.EgressWorkspaceId = null,
      missingValueState: CaseMetadataState.EgressConnectionIsNull
    );

  private async Task<ClearFolderPathResult> ClearConnectionAsync(
    int caseId,
    string logContext,
    Func<CaseMetadata, string?> getValue,
    Action<CaseMetadata> clearValue,
    CaseMetadataState missingValueState)
  {
    _logger.LogInformation("Clearing {LogContext} for case {CaseId}", logContext, caseId);
    try
    {
      var existingMetadata = await _caseMetadataRepository.GetByCaseIdAsync(caseId);

      if (existingMetadata == null)
      {
        _logger.LogWarning("No metadata found for case {CaseId} to clear {LogContext}", caseId, logContext);
        return new ClearFolderPathResult { State = CaseMetadataState.NoCaseMetadataFound };
      }

      if (existingMetadata.ActiveTransferId.HasValue)
      {
        _logger.LogWarning("Cannot clear {LogContext} for case {CaseId} because there is an active transfer", logContext, caseId);
        return new ClearFolderPathResult { State = CaseMetadataState.TransferIsActive };
      }

      var existingValue = getValue(existingMetadata);
      if (string.IsNullOrEmpty(existingValue))
      {
        _logger.LogWarning("No {LogContext} to clear for case {CaseId}", logContext, caseId);
        return new ClearFolderPathResult { State = missingValueState };
      }

      clearValue(existingMetadata);
      await _caseMetadataRepository.UpdateAsync(existingMetadata);
      return new ClearFolderPathResult { State = CaseMetadataState.Success, ClearedPath = existingValue };
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error clearing {LogContext} for case {CaseId}", logContext, caseId);
      throw;
    }
  }
}