using CPS.ComplexCases.API.Domain.Models;

namespace CPS.ComplexCases.API.Services;

public interface ISecurityGroupMetadataService
{
    Task<List<SecurityGroup>> GetUserSecurityGroupsAsync(string bearerToken);
}