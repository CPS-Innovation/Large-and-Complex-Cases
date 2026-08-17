using Microsoft.Extensions.Logging;
using AutoFixture;
using CPS.ComplexCases.API.Domain.Models;
using CPS.ComplexCases.API.Exceptions;
using CPS.ComplexCases.API.Services;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Services;

public class UserBucketAccessServiceTests
{
    private readonly Mock<ILogger<UserBucketAccessService>> _loggerMock;
    private readonly Mock<ISecurityGroupMetadataService> _securityGroupMetadataServiceMock;
    private readonly UserBucketAccessService _service;
    private readonly Fixture _fixture;
    private readonly string _bearerToken;

    private const string YorkBucket = "york-bucket";
    private const string ManchesterBucket = "manchester-bucket";

    public UserBucketAccessServiceTests()
    {
        _fixture = new Fixture();
        _loggerMock = new Mock<ILogger<UserBucketAccessService>>();
        _securityGroupMetadataServiceMock = new Mock<ISecurityGroupMetadataService>();
        _bearerToken = _fixture.Create<string>();

        _service = new UserBucketAccessService(_loggerMock.Object, _securityGroupMetadataServiceMock.Object);
    }

    [Fact]
    public async Task GetEntitledBucketsAsync_ReturnsAllEntitledBuckets()
    {
        // Arrange
        SetupEntitlements(YorkBucket, ManchesterBucket);

        // Act
        var result = await _service.GetEntitledBucketsAsync(_bearerToken);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.BucketName == YorkBucket);
        Assert.Contains(result, x => x.BucketName == ManchesterBucket);
    }

    [Fact]
    public async Task GetEntitledBucketsAsync_PropagatesMissingSecurityGroupException_WhenNoEntitlements()
    {
        // Arrange
        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(_bearerToken))
            .ThrowsAsync(new MissingSecurityGroupException("No matching security groups found for the provided IDs."));

        // Act & Assert — the exception middleware maps this to a 403
        await Assert.ThrowsAsync<MissingSecurityGroupException>(() => _service.GetEntitledBucketsAsync(_bearerToken));
    }

    [Fact]
    public async Task EnsureBucketAllowedAsync_ReturnsBucket_WhenEntitled()
    {
        // Arrange
        SetupEntitlements(YorkBucket, ManchesterBucket);

        // Act
        var result = await _service.EnsureBucketAllowedAsync(_bearerToken, ManchesterBucket);

        // Assert
        Assert.Equal(ManchesterBucket, result.BucketName);
    }

    [Fact]
    public async Task EnsureBucketAllowedAsync_IsCaseInsensitive()
    {
        // Arrange
        SetupEntitlements(YorkBucket);

        // Act
        var result = await _service.EnsureBucketAllowedAsync(_bearerToken, YorkBucket.ToUpperInvariant());

        // Assert
        Assert.Equal(YorkBucket, result.BucketName);
    }

    [Fact]
    public async Task EnsureBucketAllowedAsync_Throws_WhenNotEntitled()
    {
        // Arrange
        SetupEntitlements(YorkBucket);

        // Act & Assert
        await Assert.ThrowsAsync<MissingSecurityGroupException>(() =>
            _service.EnsureBucketAllowedAsync(_bearerToken, ManchesterBucket));
    }

    [Fact]
    public async Task ResolveBucketAsync_PrefersPersistedBucket_OverRequestedBucket()
    {
        // Arrange
        SetupEntitlements(YorkBucket, ManchesterBucket);

        // Act
        var result = await _service.ResolveBucketAsync(_bearerToken, ManchesterBucket, YorkBucket);

        // Assert
        Assert.Equal(ManchesterBucket, result.BucketName);
    }

    [Fact]
    public async Task ResolveBucketAsync_UsesRequestedBucket_WhenNothingPersisted()
    {
        // Arrange
        SetupEntitlements(YorkBucket, ManchesterBucket);

        // Act
        var result = await _service.ResolveBucketAsync(_bearerToken, null, ManchesterBucket);

        // Assert
        Assert.Equal(ManchesterBucket, result.BucketName);
    }

    [Fact]
    public async Task ResolveBucketAsync_Throws_WhenPersistedBucketNoLongerEntitled()
    {
        // Arrange — the user lost the AD group membership after connecting
        SetupEntitlements(YorkBucket);

        // Act & Assert
        await Assert.ThrowsAsync<MissingSecurityGroupException>(() =>
            _service.ResolveBucketAsync(_bearerToken, ManchesterBucket, null));
    }

    [Fact]
    public async Task ResolveBucketAsync_Throws_WhenRequestedBucketNotEntitled()
    {
        // Arrange
        SetupEntitlements(YorkBucket);

        // Act & Assert
        await Assert.ThrowsAsync<MissingSecurityGroupException>(() =>
            _service.ResolveBucketAsync(_bearerToken, null, ManchesterBucket));
    }

    [Fact]
    public async Task ResolveBucketAsync_FallsBackToFirstEntitlement_WhenNeitherSupplied()
    {
        // Arrange
        SetupEntitlements(YorkBucket);

        // Act
        var result = await _service.ResolveBucketAsync(_bearerToken, null, null);

        // Assert
        Assert.Equal(YorkBucket, result.BucketName);
    }

    [Fact]
    public async Task ResolveBucketAsync_WarnsAndFallsBackToFirst_WhenMultipleEntitlements()
    {
        // Arrange
        SetupEntitlements(YorkBucket, ManchesterBucket);

        // Act
        var result = await _service.ResolveBucketAsync(_bearerToken, null, null);

        // Assert
        Assert.Equal(YorkBucket, result.BucketName);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(YorkBucket)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ResolveBucketAsync_DoesNotWarn_WhenSingleEntitlement()
    {
        // Arrange
        SetupEntitlements(YorkBucket);

        // Act
        await _service.ResolveBucketAsync(_bearerToken, null, null);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    private void SetupEntitlements(params string[] bucketNames)
    {
        var securityGroups = bucketNames.Select(bucketName => new SecurityGroup
        {
            Id = _fixture.Create<Guid>(),
            BucketName = bucketName,
            VolumeUuid = _fixture.Create<Guid>(),
            DisplayName = bucketName
        }).ToList();

        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(_bearerToken))
            .ReturnsAsync(securityGroups);
    }
}
