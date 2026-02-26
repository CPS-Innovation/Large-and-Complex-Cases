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

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "SourceFiles/SecurityGroupMappings.PreProd.json");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(securityGroups));

        try
        {
            // Act
            var result = await _service.GetUserSecurityGroupsAsync(bearerToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, g => g.Id == groupId1);
            Assert.Contains(result, g => g.Id == groupId2);

        }
        finally
        {
            // Cleanup
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
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

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "SourceFiles/SecurityGroupMappings.PreProd.json");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(securityGroups));

        try
        {
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
        }
        finally
        {
            // Cleanup
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public async Task GetUserSecurityGroupsAsync_ThrowsException_WhenMappingFileNotFound()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var bearerToken = GenerateJwtToken(new List<string> { groupId.ToString() });

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "SourceFiles/SecurityGroupMappings.PreProd.json");
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

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "SourceFiles/SecurityGroupMappings.PreProd.json");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(filePath, "invalid json");

        try
        {
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
        }
        finally
        {
            // Cleanup
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public async Task GetUserSecurityGroupsAsync_SkipsMalformedGuids_AndReturnsValidOnes()
    {
        // Arrange
        var validGroupId = Guid.NewGuid();
        var bearerToken = GenerateJwtToken(new List<string> { "not-a-guid", validGroupId.ToString(), "also-invalid" });

        var securityGroups = new List<SecurityGroup>
        {
            new() { Id = validGroupId, DisplayName = "Group1", BucketName = "Bucket1", Description = "Test Group 1" }
        };

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "SourceFiles/SecurityGroupMappings.PreProd.json");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(securityGroups));

        try
        {
            // Act
            var result = await _service.GetUserSecurityGroupsAsync(bearerToken);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Contains(result, g => g.Id == validGroupId);

            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Malformed GUID found in token groups claim: not-a-guid")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Malformed GUID found in token groups claim: also-invalid")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        finally
        {
            // Cleanup
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public async Task GetUserSecurityGroupsAsync_ThrowsException_WhenAllGuidsAreMalformed()
    {
        // Arrange
        var bearerToken = GenerateJwtToken(new List<string> { "not-a-guid", "also-not-a-guid" });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<MissingSecurityGroupException>(() =>
            _service.GetUserSecurityGroupsAsync(bearerToken));

        Assert.Equal("No security group IDs found in the token.", exception.Message);

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Malformed GUID found in token groups claim")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Exactly(2));

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
    public async Task GetUserSecurityGroupsAsync_CachesSecurityGroups_OnSecondCall()
    {
        // Arrange
        var groupId = Guid.NewGuid();
        var bearerToken = GenerateJwtToken(new List<string> { groupId.ToString() });

        var securityGroups = new List<SecurityGroup>
        {
            new() { Id = groupId, DisplayName = "Group1", BucketName = "Bucket1", Description = "Test Group 1" }
        };

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "SourceFiles/SecurityGroupMappings.PreProd.json");
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(securityGroups));

        // Act - First call should load from file
        var result1 = await _service.GetUserSecurityGroupsAsync(bearerToken);

        // Delete the file to prove the second call uses cache
        File.Delete(filePath);

        // Act - Second call should use cached data
        var result2 = await _service.GetUserSecurityGroupsAsync(bearerToken);

        // Assert
        Assert.NotNull(result1);
        Assert.Single(result1);
        Assert.NotNull(result2);
        Assert.Single(result2);
        Assert.Equal(result1[0].Id, result2[0].Id);

        // Verify that the "loaded successfully" log appears only once
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Security group mappings loaded successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}