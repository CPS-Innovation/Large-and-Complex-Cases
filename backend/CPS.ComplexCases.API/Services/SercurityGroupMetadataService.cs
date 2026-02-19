using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using CPS.ComplexCases.API.Domain.Models;
using CPS.ComplexCases.API.Exceptions;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.API.Services;

public class SecurityGroupMetadataService(ILogger<SecurityGroupMetadataService> logger) : ISecurityGroupMetadataService
{
    private readonly ILogger<SecurityGroupMetadataService> _logger = logger;

    public async Task<List<SecurityGroup>> GetUserSecurityGroupsAsync(string bearerToken)
    {
        var groupIds = ExtractGroupIdsFromToken(bearerToken);
        var groups = await GetSecurityGroupDetails();
        var result = new List<SecurityGroup>();

        result = groups.Where(x => groupIds.Contains(x.Id)).ToList();

        if (result.Count == 0)
        {
            _logger.LogWarning("No matching security groups found for the provided IDs.");
            throw new MissingSecurityGroupException("No matching security groups found for the provided IDs.");
        }

        return result;
    }

    private List<Guid> ExtractGroupIdsFromToken(string bearerToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(bearerToken);

        var groupIds = jwt.Claims
            .Where(c => c.Type == "groups")
            .Select(c => Guid.Parse(c.Value))
            .ToList();

        if (groupIds.Count == 0)
        {
            _logger.LogWarning("No security group IDs found in the token.");
            throw new MissingSecurityGroupException("No security group IDs found in the token.");
        }

        return groupIds;
    }

    private async Task<List<SecurityGroup>> GetSecurityGroupDetails()
    {
        try
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var suffix = environment == "Production" ? "Production" : "PreProd";
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), $"SourceFiles/SecurityGroupMappings.{suffix}.json");

            if (!File.Exists(filePath))
            {
                _logger.LogError("Security group mapping file not found at path: {filePath}", filePath);
                throw new MissingSecurityGroupException($"Security group mapping file not found at path: {filePath}");
            }

            var jsonContent = await File.ReadAllTextAsync(filePath);
            var groups = JsonSerializer.Deserialize<List<SecurityGroup>>(jsonContent);

            if (groups == null)
            {
                _logger.LogError("Failed to deserialize security group data from JSON.");
                throw new MissingSecurityGroupException("Failed to deserialize security group data from JSON.");
            }

            return groups;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize security group data from JSON.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while retrieving security group details.");
            throw;
        }
    }
}