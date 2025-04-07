
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
}