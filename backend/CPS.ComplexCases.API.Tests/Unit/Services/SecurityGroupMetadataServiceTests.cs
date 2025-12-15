using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using CPS.ComplexCases.API.Domain.Models;
using CPS.ComplexCases.API.Exceptions;
using CPS.ComplexCases.API.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Services;

public class SecurityGroupMetadataServiceTests
{
    private readonly Mock<ILogger<SecurityGroupMetadataService>> _loggerMock = new();
    private readonly SecurityGroupMetadataService _service;

    public SecurityGroupMetadataServiceTests()
    {
        _service = new SecurityGroupMetadataService(_loggerMock.Object);
    }

    private static string GenerateJwtToken(List<string> groupIds)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = new JwtSecurityToken(
            issuer: "test-issuer",
            audience: "test-audience",
            claims: groupIds.ConvertAll(id => new System.Security.Claims.Claim("groups", id)),
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: null);
        return handler.WriteToken(token);
    }

    [Fact]
    public async Task GetUserSecurityGroupsAsync_ReturnsMatchingGroups_OnSuccess()
    {
        // Arrange
        var groupId1 = Guid.NewGuid();
        var groupId2 = Guid.NewGuid();
        var bearerToken = GenerateJwtToken(new List<string> { groupId1.ToString(), groupId2.ToString() });

        var securityGroups = new List<SecurityGroup>
        {
            new() { Id = groupId1, DisplayName = "Group1", BucketName = "Bucket1", Description = "Test Group 1" },
            new() { Id = groupId2, DisplayName = "Group2", BucketName = "Bucket2", Description = "Test Group 2" },
            new() { Id = Guid.NewGuid(), DisplayName = "Group3", BucketName = "Bucket3", Description = "Test Group 3" }
        };

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "SourceFiles/SecurityGroupMappings.json");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(securityGroups));

        // Act
        var result = await _service.GetUserSecurityGroupsAsync(bearerToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, g => g.Id == groupId1);
        Assert.Contains(result, g => g.Id == groupId2);

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public async Task GetUserSecurityGroupsAsync_ThrowsException_WhenNoGroupIdsInToken()
    {
        // Arrange
        var handler = new JwtSecurityTokenHandler();
        var token = new JwtSecurityToken(
            issuer: "test-issuer",
            audience: "test-audience",
            claims: new List<System.Security.Claims.Claim>(),
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: null);
        var bearerToken = handler.WriteToken(token);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<MissingSecurityGroupException>(() =>
            _service.GetUserSecurityGroupsAsync(bearerToken));

        Assert.Equal("No security group IDs found in the token.", exception.Message);
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No security group IDs found in the token")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetUserSecurityGroupsAsync_ThrowsException_WhenNoMatchingGroupsFound()
    {
        // Arrange
        var groupId1 = Guid.NewGuid();
        var groupId2 = Guid.NewGuid();
        var bearerToken = GenerateJwtToken(new List<string> { groupId1.ToString() });

        var securityGroups = new List<SecurityGroup>
        {
            new () { Id = groupId2, DisplayName = "Group1", BucketName = "Bucket1", Description = "Test Group 1" }
        };

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "SourceFiles/SecurityGroupMappings.json");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(securityGroups));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<MissingSecurityGroupException>(() =>
            _service.GetUserSecurityGroupsAsync(bearerToken));

        Assert.Equal("No matching security groups found for the provided IDs.", exception.Message);
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No matching security groups found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public async Task GetUserSecurityGroupsAsync_ThrowsException_WhenMappingFileNotFound()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var bearerToken = GenerateJwtToken(new List<string> { groupId.ToString() });

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "SourceFiles/SecurityGroupMappings.json");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        // Act & Assert
        var exception = await Assert.ThrowsAsync<MissingSecurityGroupException>(() =>
            _service.GetUserSecurityGroupsAsync(bearerToken));

        Assert.Contains("Security group mapping file not found", exception.Message);
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Security group mapping file not found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetUserSecurityGroupsAsync_ThrowsException_WhenJsonDeserializationFails()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var bearerToken = GenerateJwtToken(new List<string> { groupId.ToString() });

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "SourceFiles/SecurityGroupMappings.json");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(filePath, "invalid json");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<JsonException>(() =>
            _service.GetUserSecurityGroupsAsync(bearerToken));

        Assert.Equal("'i' is an invalid start of a value. Path: $ | LineNumber: 0 | BytePositionInLine: 0.", exception.Message);
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to deserialize security group data from JSON.")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Cleanup
        File.Delete(filePath);
    }

    [Fact]
    public void ExtractGroupIdsFromToken_ReturnsGroupIds_WhenValid()
    {
        // Arrange
        var groupId1 = Guid.NewGuid();
        var groupId2 = Guid.NewGuid();
        var bearerToken = GenerateJwtToken(new List<string> { groupId1.ToString(), groupId2.ToString() });

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(bearerToken);

        var groupIds = jwt.Claims
            .Where(c => c.Type == "groups")
            .Select(c => Guid.Parse(c.Value))
            .ToList();

        // Assert
        Assert.Equal(2, groupIds.Count);
        Assert.Contains(groupId1, groupIds);
        Assert.Contains(groupId2, groupIds);
    }
}