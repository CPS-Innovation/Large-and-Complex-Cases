using AutoFixture;
using CPS.ComplexCases.API.Constants;
using CPS.ComplexCases.API.Domain.Models;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.API.Services;
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
    private readonly Mock<ISecurityGroupMetadataService> _securityGroupMetadataServiceMock;
    private readonly ListNetAppFiles _function;
    private readonly Fixture _fixture;
    private readonly string _testBearerToken;
    private readonly string _testBucketName;
    private readonly Guid _testCorrelationId;
    private readonly string _testUsername;
    private readonly string _testCmsAuthValues;

    public ListNetAppFilesTests()
    {
        _loggerMock = new Mock<ILogger<ListNetAppFiles>>();
        _netAppClientMock = new Mock<INetAppClient>();
        _netAppArgFactoryMock = new Mock<INetAppArgFactory>();
        _optionsMock = new Mock<IOptions<NetAppOptions>>();
        _securityGroupMetadataServiceMock = new Mock<ISecurityGroupMetadataService>();
        _fixture = new Fixture();

        _testBearerToken = _fixture.Create<string>();
        _testBucketName = _fixture.Create<string>();
        _testCorrelationId = _fixture.Create<Guid>();
        _testUsername = _fixture.Create<string>();
        _testCmsAuthValues = _fixture.Create<string>();

        _securityGroupMetadataServiceMock
                .Setup(s => s.GetUserSecurityGroupsAsync(It.IsAny<string>()))
                .ReturnsAsync([
                    new SecurityGroup
                    {
                        Id = _fixture.Create<Guid>(),
                        BucketName = _testBucketName,
                        DisplayName = "Test Security Group"
                    }
                ]);

        _optionsMock.Setup(o => o.Value).Returns(new NetAppOptions
        {
            Url = "https://example.com",
            RegionName = "test-region"
        });

        _function = new ListNetAppFiles(
            _loggerMock.Object,
            _netAppClientMock.Object,
            _netAppArgFactoryMock.Object,
            _optionsMock.Object,
            _securityGroupMetadataServiceMock.Object);
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
                _testBearerToken,
                _testBucketName,
                queryParams[InputParameters.ContinuationToken],
                50,
                queryParams[InputParameters.Path],
                true))
            .Returns(arg);

        _netAppClientMock
            .Setup(c => c.ListObjectsInBucketAsync(arg))
            .ReturnsAsync(response);

        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, okResult.Value);

        _netAppArgFactoryMock.Verify(f => f.CreateListObjectsInBucketArg(
            _testBearerToken,
            _testBucketName,
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
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<bool>()))
            .Returns(arg);

        _netAppClientMock
            .Setup(c => c.ListObjectsInBucketAsync(arg))
            .ReturnsAsync((ListNetAppObjectsDto?)null);

        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        var result = await _function.Run(httpRequest, functionContext);
        // Assert
        Assert.IsType<BadRequestResult>(result);
    }
}
