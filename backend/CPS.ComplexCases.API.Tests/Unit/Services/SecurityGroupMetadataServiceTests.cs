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

    private static string CreateTempJsonFile(string content)
    {
        var filePath = Path.Combine(
            Path.GetTempPath(),
            $"SecurityGroupTest_{Guid.NewGuid()}.json");

        File.WriteAllText(filePath, content);

        return filePath;
    }

    [Theory]
    [InlineData("Production", "eu-south-1", "SecurityGroupMappings.Production.eu-south-1.json")]
    [InlineData("Production", "eu-west-1", "SecurityGroupMappings.Production.eu-west-1.json")]
    [InlineData("Production", null, "SecurityGroupMappings.Production.eu-south-1.json")]
    [InlineData("Production", "   ", "SecurityGroupMappings.Production.eu-south-1.json")]
    [InlineData("Production", "EU-SOUTH-1", "SecurityGroupMappings.Production.eu-south-1.json")]
    [InlineData("PreProd", "eu-south-1", "SecurityGroupMappings.PreProd.eu-west-1.json")]
    [InlineData("PreProd", "eu-west-1", "SecurityGroupMappings.PreProd.eu-west-1.json")]
    [InlineData("Development", null, "SecurityGroupMappings.PreProd.eu-west-1.json")]
    [InlineData(null, null, "SecurityGroupMappings.PreProd.eu-west-1.json")]
    public void BuildSecurityGroupFilePath_ReturnsExpectedPath(
        // Arrange
        string? environment,
        string? region,
        string expectedFileName)
    {
        // Act
        var result = SecurityGroupMetadataService.BuildSecurityGroupFilePath(environment, region);

        // Assert
        var expected = Path.Combine(
            Directory.GetCurrentDirectory(),
            "SourceFiles",
            expectedFileName);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("SecurityGroupMappings.Production.eu-west-1.json")]
    [InlineData("SecurityGroupMappings.Production.eu-south-1.json")]
    [InlineData("SecurityGroupMappings.PreProd.eu-west-1.json")]
    public void BundledMappingFiles_AreValidAndDeserializable(string fileName)
    {
        var filePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "SourceFiles",
            fileName);

        Assert.True(File.Exists(filePath), $"File {fileName} does not exist.");

        var json = File.ReadAllText(filePath);
        var groups = JsonSerializer.Deserialize<List<SecurityGroup>>(json);

        Assert.NotNull(groups);
        Assert.NotEmpty(groups);
        Assert.All(groups, g =>
        {
            Assert.NotEqual(Guid.Empty, g.Id);
            Assert.NotEqual(Guid.Empty, g.VolumeUuid);
            Assert.False(string.IsNullOrWhiteSpace(g.BucketName));
        });
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
            new() { Id = groupId1, DisplayName = "Group1", BucketName = "Bucket1", VolumeUuid = Guid.NewGuid(), Description = "Test Group 1" },
            new() { Id = groupId2, DisplayName = "Group2", BucketName = "Bucket2", VolumeUuid = Guid.NewGuid(), Description = "Test Group 2" },
            new() { Id = Guid.NewGuid(), DisplayName = "Group3", BucketName = "Bucket3", VolumeUuid = Guid.NewGuid(), Description = "Test Group 3" }
        };

        var filePath = CreateTempJsonFile(
            JsonSerializer.Serialize(securityGroups));

        var service = new SecurityGroupMetadataService(
            _loggerMock.Object,
            filePath);

        try
        {
            // Act
            var result = await service.GetUserSecurityGroupsAsync(bearerToken);

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
            new () { Id = groupId2, DisplayName = "Group1", BucketName = "Bucket1", VolumeUuid = Guid.NewGuid(), Description = "Test Group 1" }
        };

        var filePath = CreateTempJsonFile(
            JsonSerializer.Serialize(securityGroups));

        var service = new SecurityGroupMetadataService(
            _loggerMock.Object,
            filePath);


        try
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<MissingSecurityGroupException>(() =>
                service.GetUserSecurityGroupsAsync(bearerToken));

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

        var filePath = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid()}.json");

        var service = new SecurityGroupMetadataService(
            _loggerMock.Object,
            filePath);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<MissingSecurityGroupException>(() =>
            service.GetUserSecurityGroupsAsync(bearerToken));

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

        var filePath = CreateTempJsonFile("invalid json");

        var service = new SecurityGroupMetadataService(
            _loggerMock.Object,
            filePath);

        try
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<JsonException>(() =>
                service.GetUserSecurityGroupsAsync(bearerToken));

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
            new() { Id = validGroupId, DisplayName = "Group1", BucketName = "Bucket1", VolumeUuid = Guid.NewGuid(), Description = "Test Group 1" }
        };

        var filePath = CreateTempJsonFile(
            JsonSerializer.Serialize(securityGroups));

        var service = new SecurityGroupMetadataService(
            _loggerMock.Object,
            filePath);

        try
        {
            // Act
            var result = await service.GetUserSecurityGroupsAsync(bearerToken);

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
            new() { Id = groupId, DisplayName = "Group1", BucketName = "Bucket1", VolumeUuid = Guid.NewGuid(), Description = "Test Group 1" }
        };

        var filePath = CreateTempJsonFile(
            JsonSerializer.Serialize(securityGroups));

        var service = new SecurityGroupMetadataService(
            _loggerMock.Object,
            filePath);

        // Act - First call should load from file
        var result1 = await service.GetUserSecurityGroupsAsync(bearerToken);

        // Delete the file to prove the second call uses cache
        File.Delete(filePath);

        // Act - Second call should use cached data
        var result2 = await service.GetUserSecurityGroupsAsync(bearerToken);

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
