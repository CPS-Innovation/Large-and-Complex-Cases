using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AutoFixture;
using CPS.ComplexCases.API.Domain.Models;
using CPS.ComplexCases.API.Exceptions;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models.Args;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions;

public class CreateNetAppFolderTests
{
    private readonly Mock<ILogger<CreateNetAppFolder>> _loggerMock;
    private readonly Mock<INetAppClient> _netAppClientMock;
    private readonly Mock<INetAppArgFactory> _netAppArgFactoryMock;
    private readonly Mock<ISecurityGroupMetadataService> _securityGroupMetadataServiceMock;
    private readonly Mock<IInitializationHandler> _initializationHandlerMock;
    private readonly CreateNetAppFolder _function;
    private readonly Fixture _fixture;
    private readonly string _testBearerToken;
    private readonly string _testBucketName;
    private readonly Guid _testCorrelationId;
    private readonly string _testUsername;
    private readonly string _testCmsAuthValues;

    public CreateNetAppFolderTests()
    {
        _loggerMock = new Mock<ILogger<CreateNetAppFolder>>();
        _netAppClientMock = new Mock<INetAppClient>();
        _netAppArgFactoryMock = new Mock<INetAppArgFactory>();
        _securityGroupMetadataServiceMock = new Mock<ISecurityGroupMetadataService>();
        _initializationHandlerMock = new Mock<IInitializationHandler>();

        _fixture = new Fixture();
        _testBearerToken = _fixture.Create<string>();
        _testBucketName = _fixture.Create<string>();
        _testCorrelationId = _fixture.Create<Guid>();
        _testUsername = _fixture.Create<string>();
        _testCmsAuthValues = _fixture.Create<string>();

        _function = new CreateNetAppFolder(
            _loggerMock.Object,
            _netAppClientMock.Object,
            _netAppArgFactoryMock.Object,
            _securityGroupMetadataServiceMock.Object,
            _initializationHandlerMock.Object);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Run_WhenOperationNameIsNullOrWhitespace_ReturnsBadRequestObjectResult(string operationName)
    {
        // Arrange
        var httpRequest = new DefaultHttpContext().Request;
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        var result = await _function.Run(httpRequest, operationName, functionContext);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Operation name cannot be empty", badRequestResult.Value);

        _initializationHandlerMock.Verify(h => h.Initialize(It.IsAny<string>(), It.IsAny<Guid>(), null), Times.Once);
        _securityGroupMetadataServiceMock.Verify(s => s.GetUserSecurityGroupsAsync(It.IsAny<string>()), Times.Never);
        _netAppArgFactoryMock.Verify(f => f.CreateCreateFolderArg(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _netAppClientMock.Verify(c => c.CreateFolderAsync(It.IsAny<CreateFolderArg>()), Times.Never);
    }

    [Theory]
    [InlineData("..")]
    [InlineData("../folder")]
    [InlineData("operation/..")]
    [InlineData("/operation")]
    [InlineData("/")]
    public async Task Run_WhenOperationNameContainsDotDotOrStartsWithSlash_ReturnsBadRequestObjectResult(string operationName)
    {
        // Arrange
        var httpRequest = new DefaultHttpContext().Request;
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        var result = await _function.Run(httpRequest, operationName, functionContext);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid operation name", badRequestResult.Value);

        _initializationHandlerMock.Verify(h => h.Initialize(It.IsAny<string>(), It.IsAny<Guid>(), null), Times.Once);
        _securityGroupMetadataServiceMock.Verify(s => s.GetUserSecurityGroupsAsync(It.IsAny<string>()), Times.Never);
        _netAppArgFactoryMock.Verify(f => f.CreateCreateFolderArg(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _netAppClientMock.Verify(c => c.CreateFolderAsync(It.IsAny<CreateFolderArg>()), Times.Never);
    }

    [Fact]
    public async Task Run_WhenFolderCreatedSuccessfully_ReturnsOkObjectResultWithTrue()
    {
        // Arrange
        var operationName = _fixture.Create<string>();
        var arg = _fixture.Create<CreateFolderArg>();
        var securityGroups = new List<SecurityGroup>
        {
            new() {
                Id = _fixture.Create<Guid>(),
                BucketName = _testBucketName,
                DisplayName = "Test Security Group"
            }
        };

        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(_testBearerToken))
            .ReturnsAsync(securityGroups);

        _netAppArgFactoryMock
            .Setup(f => f.CreateCreateFolderArg(_testBearerToken, _testBucketName, operationName))
            .Returns(arg);

        _netAppClientMock
            .Setup(c => c.CreateFolderAsync(arg))
            .ReturnsAsync(true);

        var httpRequest = new DefaultHttpContext().Request;
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        var result = await _function.Run(httpRequest, operationName, functionContext);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.True((bool)okResult.Value!);

        _initializationHandlerMock.Verify(h => h.Initialize(_testUsername, _testCorrelationId, null), Times.Once);
        _securityGroupMetadataServiceMock.Verify(s => s.GetUserSecurityGroupsAsync(_testBearerToken), Times.Once);
        _netAppArgFactoryMock.Verify(f => f.CreateCreateFolderArg(_testBearerToken, _testBucketName, operationName), Times.Once);
        _netAppClientMock.Verify(c => c.CreateFolderAsync(arg), Times.Once);
    }

    [Fact]
    public async Task Run_WhenFolderCreationFails_ReturnsOkObjectResultWithFalse()
    {
        // Arrange
        var operationName = _fixture.Create<string>();
        var arg = _fixture.Create<CreateFolderArg>();
        var securityGroups = new List<SecurityGroup>
        {
            new() {
                Id = _fixture.Create<Guid>(),
                BucketName = _testBucketName,
                DisplayName = "Test Security Group"
            }
        };

        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(_testBearerToken))
            .ReturnsAsync(securityGroups);

        _netAppArgFactoryMock
            .Setup(f => f.CreateCreateFolderArg(_testBearerToken, _testBucketName, operationName))
            .Returns(arg);

        _netAppClientMock
            .Setup(c => c.CreateFolderAsync(arg))
            .ReturnsAsync(false);

        var httpRequest = new DefaultHttpContext().Request;
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        var result = await _function.Run(httpRequest, operationName, functionContext);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.False((bool)okResult.Value!);

        _initializationHandlerMock.Verify(h => h.Initialize(_testUsername, _testCorrelationId, null), Times.Once);
        _securityGroupMetadataServiceMock.Verify(s => s.GetUserSecurityGroupsAsync(_testBearerToken), Times.Once);
        _netAppArgFactoryMock.Verify(f => f.CreateCreateFolderArg(_testBearerToken, _testBucketName, operationName), Times.Once);
        _netAppClientMock.Verify(c => c.CreateFolderAsync(arg), Times.Once);
    }

    [Fact]
    public async Task Run_WhenNoSecurityGroupsFound_ThrowsMissingSecurityGroupException()
    {
        // Arrange
        var operationName = _fixture.Create<string>();

        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(_testBearerToken))
            .ThrowsAsync(new MissingSecurityGroupException("No matching security groups found for the provided IDs."));

        var httpRequest = new DefaultHttpContext().Request;
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act & Assert
        await Assert.ThrowsAsync<MissingSecurityGroupException>(() =>
            _function.Run(httpRequest, operationName, functionContext));

        _initializationHandlerMock.Verify(h => h.Initialize(_testUsername, _testCorrelationId, null), Times.Once);
        _securityGroupMetadataServiceMock.Verify(s => s.GetUserSecurityGroupsAsync(_testBearerToken), Times.Once);
        _netAppArgFactoryMock.Verify(f => f.CreateCreateFolderArg(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _netAppClientMock.Verify(c => c.CreateFolderAsync(It.IsAny<CreateFolderArg>()), Times.Never);
    }

    [Fact]
    public async Task Run_InitializesHandlerWithCorrectParameters()
    {
        // Arrange
        var operationName = _fixture.Create<string>();
        var arg = _fixture.Create<CreateFolderArg>();
        var securityGroups = new List<SecurityGroup>
        {
            new() {
                Id = _fixture.Create<Guid>(),
                BucketName = _testBucketName,
                DisplayName = "Test Security Group"
            }
        };

        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(_testBearerToken))
            .ReturnsAsync(securityGroups);

        _netAppArgFactoryMock
            .Setup(f => f.CreateCreateFolderArg(_testBearerToken, _testBucketName, operationName))
            .Returns(arg);

        _netAppClientMock
            .Setup(c => c.CreateFolderAsync(arg))
            .ReturnsAsync(true);

        var httpRequest = new DefaultHttpContext().Request;
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        await _function.Run(httpRequest, operationName, functionContext);

        // Assert
        _initializationHandlerMock.Verify(h => h.Initialize(_testUsername, _testCorrelationId, null), Times.Once);
    }

    [Fact]
    public async Task Run_UsesFirstSecurityGroupBucketName()
    {
        // Arrange
        var operationName = _fixture.Create<string>();
        var firstBucketName = "first-bucket";
        var secondBucketName = "second-bucket";
        var arg = _fixture.Create<CreateFolderArg>();

        var securityGroups = new List<SecurityGroup>
        {
            new() {
                Id = _fixture.Create<Guid>(),
                BucketName = firstBucketName,
                DisplayName = "First Security Group"
            },
            new() {
                Id = _fixture.Create<Guid>(),
                BucketName = secondBucketName,
                DisplayName = "Second Security Group"
            }
        };

        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(_testBearerToken))
            .ReturnsAsync(securityGroups);

        _netAppArgFactoryMock
            .Setup(f => f.CreateCreateFolderArg(_testBearerToken, firstBucketName, operationName))
            .Returns(arg);

        _netAppClientMock
            .Setup(c => c.CreateFolderAsync(arg))
            .ReturnsAsync(true);

        var httpRequest = new DefaultHttpContext().Request;
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        await _function.Run(httpRequest, operationName, functionContext);

        // Assert
        _netAppArgFactoryMock.Verify(f => f.CreateCreateFolderArg(_testBearerToken, firstBucketName, operationName), Times.Once);
    }
}
