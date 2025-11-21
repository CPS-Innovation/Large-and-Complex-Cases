using AutoFixture;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions;

public class ListNetAppFilesTests
{
    private readonly Mock<ILogger<ListNetAppFiles>> _loggerMock;
    private readonly Mock<INetAppClient> _netAppClientMock;
    private readonly Mock<INetAppArgFactory> _netAppArgFactoryMock;
    private readonly Mock<IOptions<NetAppOptions>> _optionsMock;
    private readonly ListNetAppFiles _function;
    private readonly Fixture _fixture;

    public ListNetAppFilesTests()
    {
        _loggerMock = new Mock<ILogger<ListNetAppFiles>>();
        _netAppClientMock = new Mock<INetAppClient>();
        _netAppArgFactoryMock = new Mock<INetAppArgFactory>();
        _optionsMock = new Mock<IOptions<NetAppOptions>>();
        _fixture = new Fixture();

        _optionsMock.Setup(o => o.Value).Returns(new NetAppOptions
        {
            BucketName = "test-bucket",
            Url = "https://example.com",
            AccessKey = "test-access-key",
            SecretKey = "test-secret-key",
            RegionName = "test-region"
        });

        _function = new ListNetAppFiles(
            _loggerMock.Object,
            _netAppClientMock.Object,
            _netAppArgFactoryMock.Object,
            _optionsMock.Object);
    }

    [Fact]
    public async Task Run_ReturnsOkObjectResult_WhenResponseIsNotNull()
    {
        // Arrange
        var queryParams = new Dictionary<string, string>
        {
            [InputParameters.ContinuationToken] = "token",
            [InputParameters.Take] = "50",
            [InputParameters.Path] = "/some/path"
        };
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams);

        var arg = _fixture.Create<ListObjectsInBucketArg>();
        var response = _fixture.Create<ListNetAppObjectsDto>();
        var correlationId = _fixture.Create<Guid>();
        var username = _fixture.Create<string>();

        _netAppArgFactoryMock
            .Setup(f => f.CreateListObjectsInBucketArg(
                "test-bucket",
                queryParams[InputParameters.ContinuationToken],
                50,
                queryParams[InputParameters.Path],
                true))
            .Returns(arg);

        _netAppClientMock
            .Setup(c => c.ListObjectsInBucketAsync(arg))
            .ReturnsAsync(response);

        // Act
        var result = await _function.Run(httpRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, okResult.Value);

        _netAppArgFactoryMock.Verify(f => f.CreateListObjectsInBucketArg(
            "test-bucket",
            queryParams[InputParameters.ContinuationToken],
            50,
            queryParams[InputParameters.Path],
            true), Times.Once);

        _netAppClientMock.Verify(c => c.ListObjectsInBucketAsync(arg), Times.Once);
    }

    [Fact]
    public async Task Run_ReturnsBadRequest_WhenResponseIsNull()
    {
        // Arrange
        var queryParams = new Dictionary<string, string>
        {
            [InputParameters.ContinuationToken] = "token",
            [InputParameters.Take] = "50",
            [InputParameters.Path] = "/some/path"
        };
        var httpRequest = HttpRequestStubHelper.CreateHttpRequestWithQueryParameters(queryParams);

        var arg = _fixture.Create<ListObjectsInBucketArg>();
        var correlationId = _fixture.Create<Guid>();
        var username = _fixture.Create<string>();

        _netAppArgFactoryMock
            .Setup(f => f.CreateListObjectsInBucketArg(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<bool>()))
            .Returns(arg);

        _netAppClientMock
            .Setup(c => c.ListObjectsInBucketAsync(arg))
            .ReturnsAsync((ListNetAppObjectsDto?)null);

        // Act
        var result = await _function.Run(httpRequest);
        // Assert
        Assert.IsType<BadRequestResult>(result);
    }
}
