using Amazon.S3.Model;
using AutoFixture;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models.Args;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions;

public class CreateNetAppFolderTests
{
    private readonly Mock<ILogger<CreateNetAppFolder>> _loggerMock;
    private readonly Mock<INetAppClient> _netAppClientMock;
    private readonly Mock<INetAppArgFactory> _netAppArgFactoryMock;
    private readonly CreateNetAppFolder _function;
    private readonly Fixture _fixture;
    private readonly string _testBearerToken;
    private readonly Guid _testCorrelationId;
    private readonly string _testUsername;
    private readonly string _testCmsAuthValues;

    public CreateNetAppFolderTests()
    {
        _loggerMock = new Mock<ILogger<CreateNetAppFolder>>();
        _netAppClientMock = new Mock<INetAppClient>();
        _netAppArgFactoryMock = new Mock<INetAppArgFactory>();

        _function = new CreateNetAppFolder(
            _loggerMock.Object,
            _netAppClientMock.Object,
            _netAppArgFactoryMock.Object);

        _fixture = new Fixture();
        _testBearerToken = _fixture.Create<string>();
        _testCorrelationId = _fixture.Create<Guid>();
        _testUsername = _fixture.Create<string>();
        _testCmsAuthValues = _fixture.Create<string>();
    }

    [Fact]
    public async Task Run_WhenBucketFound_ReturnsOkObjectResult()
    {
        // Arrange
        var operationName = "op123";
        var arg = _fixture.Create<FindBucketArg>();
        var bucketResult = new S3Bucket
        {
            BucketName = "test-bucket",
            CreationDate = DateTime.UtcNow
        };

        _netAppArgFactoryMock
            .Setup(f => f.CreateFindBucketArg(_testBearerToken, operationName))
            .Returns(arg);

        _netAppClientMock
            .Setup(c => c.FindBucketAsync(arg))
            .ReturnsAsync(bucketResult);

        var httpRequest = new DefaultHttpContext().Request;
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        var result = await _function.Run(httpRequest, operationName, functionContext);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(bucketResult, okResult.Value);

        _netAppArgFactoryMock.VerifyAll();
        _netAppClientMock.VerifyAll();
    }

    [Fact]
    public async Task Run_WhenBucketNotFound_ReturnsNotFoundObjectResult()
    {
        // Arrange
        var operationName = "missing-op";
        var arg = _fixture.Create<FindBucketArg>();

        _netAppArgFactoryMock
            .Setup(f => f.CreateFindBucketArg(_testBearerToken, operationName))
            .Returns(arg);

        _netAppClientMock
            .Setup(c => c.FindBucketAsync(arg))
            .ReturnsAsync((S3Bucket?)null);

        var httpRequest = new DefaultHttpContext().Request;
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        var result = await _function.Run(httpRequest, operationName, functionContext);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal($"Bucket {operationName} not found", notFoundResult.Value);

        _netAppArgFactoryMock.VerifyAll();
        _netAppClientMock.VerifyAll();
    }
}
