using AutoFixture;
using CPS.ComplexCases.API.Domain.Models;
using CPS.ComplexCases.API.Domain.Response;
using CPS.ComplexCases.API.Exceptions;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using CPS.ComplexCases.Common.Handlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions;

public class GetUserBucketsTests
{
    private readonly Mock<ILogger<GetUserBuckets>> _loggerMock;
    private readonly Mock<IUserBucketAccessService> _userBucketAccessServiceMock;
    private readonly Mock<IInitializationHandler> _initializationHandlerMock;
    private readonly GetUserBuckets _function;
    private readonly Fixture _fixture;
    private readonly Guid _testCorrelationId;
    private readonly string _testUsername;
    private readonly string _testCmsAuthValues;
    private readonly string _testBearerToken;

    public GetUserBucketsTests()
    {
        _fixture = new Fixture();
        _loggerMock = new Mock<ILogger<GetUserBuckets>>();
        _userBucketAccessServiceMock = new Mock<IUserBucketAccessService>();
        _initializationHandlerMock = new Mock<IInitializationHandler>();

        _testCorrelationId = _fixture.Create<Guid>();
        _testUsername = _fixture.Create<string>();
        _testCmsAuthValues = _fixture.Create<string>();
        _testBearerToken = _fixture.Create<string>();

        _function = new GetUserBuckets(
            _loggerMock.Object,
            _userBucketAccessServiceMock.Object,
            _initializationHandlerMock.Object);
    }

    [Fact]
    public async Task Run_WhenUserEntitledToOneBucket_ReturnsSingleEntry()
    {
        // Arrange
        var bucket = MakeSecurityGroup("york-bucket", "York");

        _userBucketAccessServiceMock
            .Setup(s => s.GetEntitledBucketsAsync(_testBearerToken))
            .ReturnsAsync([bucket]);

        // Act
        var result = await _function.Run(
            HttpRequestStubHelper.CreateHttpRequest(_testCorrelationId),
            CreateFunctionContext());

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ListUserBucketsResponse>(okResult.Value);

        var entry = Assert.Single(response.Buckets);
        Assert.Equal(bucket.Id, entry.Id);
        Assert.Equal("york-bucket", entry.Name);
        Assert.Equal("York", entry.DisplayName);
    }

    [Fact]
    public async Task Run_WhenUserEntitledToTwoBuckets_ReturnsBothEntries()
    {
        // Arrange
        _userBucketAccessServiceMock
            .Setup(s => s.GetEntitledBucketsAsync(_testBearerToken))
            .ReturnsAsync([
                MakeSecurityGroup("york-bucket", "York"),
                MakeSecurityGroup("manchester-bucket", "Manchester")
            ]);

        // Act
        var result = await _function.Run(
            HttpRequestStubHelper.CreateHttpRequest(_testCorrelationId),
            CreateFunctionContext());

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ListUserBucketsResponse>(okResult.Value);

        Assert.Collection(response.Buckets,
            first =>
            {
                Assert.Equal("york-bucket", first.Name);
                Assert.Equal("York", first.DisplayName);
            },
            second =>
            {
                Assert.Equal("manchester-bucket", second.Name);
                Assert.Equal("Manchester", second.DisplayName);
            });
    }

    [Fact]
    public async Task Run_WhenUserHasNoGroupMatch_ThrowsMissingSecurityGroupException()
    {
        // Arrange
        _userBucketAccessServiceMock
            .Setup(s => s.GetEntitledBucketsAsync(_testBearerToken))
            .ThrowsAsync(new MissingSecurityGroupException("No matching security groups found for the provided IDs."));

        // Act & Assert — the exception middleware maps this to a 403
        await Assert.ThrowsAsync<MissingSecurityGroupException>(() => _function.Run(
            HttpRequestStubHelper.CreateHttpRequest(_testCorrelationId),
            CreateFunctionContext()));
    }

    [Fact]
    public async Task Run_InitializesHandlerWithUsernameAndCorrelationId()
    {
        // Arrange
        _userBucketAccessServiceMock
            .Setup(s => s.GetEntitledBucketsAsync(_testBearerToken))
            .ReturnsAsync([MakeSecurityGroup("york-bucket", "York")]);

        // Act
        await _function.Run(
            HttpRequestStubHelper.CreateHttpRequest(_testCorrelationId),
            CreateFunctionContext());

        // Assert
        _initializationHandlerMock.Verify(h => h.Initialize(_testUsername, _testCorrelationId, null), Times.Once);
    }

    private SecurityGroup MakeSecurityGroup(string bucketName, string displayName) => new()
    {
        Id = _fixture.Create<Guid>(),
        BucketName = bucketName,
        VolumeUuid = _fixture.Create<Guid>(),
        DisplayName = displayName
    };

    private Microsoft.Azure.Functions.Worker.FunctionContext CreateFunctionContext() =>
        FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);
}
