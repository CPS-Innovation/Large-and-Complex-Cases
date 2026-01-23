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
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models;
using CPS.ComplexCases.Data.Models.Requests;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models.Args;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions;

public class DeleteNetAppFileOrFolderTests
{
    private readonly Mock<ILogger<DeleteNetAppFileOrFolder>> _loggerMock;
    private readonly Mock<INetAppClient> _netAppClientMock;
    private readonly Mock<INetAppArgFactory> _netAppArgFactoryMock;
    private readonly Mock<IRequestValidator> _requestValidatorMock;
    private readonly Mock<ISecurityGroupMetadataService> _securityGroupMetadataServiceMock;
    private readonly Mock<IInitializationHandler> _initializationHandlerMock;
    private readonly DeleteNetAppFileOrFolder _function;
    private readonly Fixture _fixture;
    private readonly string _testBearerToken;
    private readonly string _testBucketName;
    private readonly Guid _testCorrelationId;
    private readonly string _testUsername;
    private readonly string _testCmsAuthValues;

    public DeleteNetAppFileOrFolderTests()
    {
        _loggerMock = new Mock<ILogger<DeleteNetAppFileOrFolder>>();
        _netAppClientMock = new Mock<INetAppClient>();
        _netAppArgFactoryMock = new Mock<INetAppArgFactory>();
        _requestValidatorMock = new Mock<IRequestValidator>();
        _securityGroupMetadataServiceMock = new Mock<ISecurityGroupMetadataService>();
        _initializationHandlerMock = new Mock<IInitializationHandler>();

        _fixture = new Fixture();
        _testBearerToken = _fixture.Create<string>();
        _testBucketName = _fixture.Create<string>();
        _testCorrelationId = _fixture.Create<Guid>();
        _testUsername = _fixture.Create<string>();
        _testCmsAuthValues = _fixture.Create<string>();

        _function = new DeleteNetAppFileOrFolder(
            _loggerMock.Object,
            _netAppClientMock.Object,
            _netAppArgFactoryMock.Object,
            _requestValidatorMock.Object,
            _securityGroupMetadataServiceMock.Object,
            _initializationHandlerMock.Object);
    }

    [Fact]
    public async Task Run_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var operationName = _fixture.Create<string>();
        var validationErrors = _fixture.CreateMany<string>(2).ToList();
        var deleteRequest = _fixture.Create<DeleteNetAppFileOrFolderDto>();

        _requestValidatorMock
            .Setup(x => x.GetJsonBody<DeleteNetAppFileOrFolderDto, DeleteNetAppFileOrFolderRequestValidator>(It.IsAny<HttpRequest>()))
            .ReturnsAsync(new ValidatableRequest<DeleteNetAppFileOrFolderDto>
            {
                IsValid = false,
                ValidationErrors = validationErrors,
                Value = deleteRequest
            });

        var request = HttpRequestStubHelper.CreateHttpRequestFor(deleteRequest);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        var result = await _function.Run(request, operationName, functionContext);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errors = Assert.IsAssignableFrom<IEnumerable<string>>(badRequestResult.Value);
        Assert.Equal(validationErrors, errors);

        _initializationHandlerMock.Verify(h => h.Initialize(_testUsername, _testCorrelationId, null), Times.Once);
        _securityGroupMetadataServiceMock.Verify(s => s.GetUserSecurityGroupsAsync(It.IsAny<string>()), Times.Never);
        _netAppClientMock.Verify(c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Run_WhenOperationNameIsNullOrWhitespace_ReturnsBadRequestObjectResult(string operationName)
    {
        // Arrange
        var deleteRequest = _fixture.Create<DeleteNetAppFileOrFolderDto>();

        _requestValidatorMock
            .Setup(x => x.GetJsonBody<DeleteNetAppFileOrFolderDto, DeleteNetAppFileOrFolderRequestValidator>(It.IsAny<HttpRequest>()))
            .ReturnsAsync(new ValidatableRequest<DeleteNetAppFileOrFolderDto>
            {
                IsValid = true,
                Value = deleteRequest
            });

        var httpRequest = HttpRequestStubHelper.CreateHttpRequestFor(deleteRequest);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        var result = await _function.Run(httpRequest, operationName, functionContext);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Operation name cannot be empty", badRequestResult.Value);

        _initializationHandlerMock.Verify(h => h.Initialize(It.IsAny<string>(), It.IsAny<Guid>(), null), Times.Once);
        _securityGroupMetadataServiceMock.Verify(s => s.GetUserSecurityGroupsAsync(It.IsAny<string>()), Times.Never);
        _netAppArgFactoryMock.Verify(f => f.CreateDeleteFileOrFolderArg(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _netAppClientMock.Verify(c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()), Times.Never);
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
        var deleteRequest = _fixture.Create<DeleteNetAppFileOrFolderDto>();

        _requestValidatorMock
            .Setup(x => x.GetJsonBody<DeleteNetAppFileOrFolderDto, DeleteNetAppFileOrFolderRequestValidator>(It.IsAny<HttpRequest>()))
            .ReturnsAsync(new ValidatableRequest<DeleteNetAppFileOrFolderDto>
            {
                IsValid = true,
                Value = deleteRequest
            });

        var httpRequest = HttpRequestStubHelper.CreateHttpRequestFor(deleteRequest);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        var result = await _function.Run(httpRequest, operationName, functionContext);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid operation name", badRequestResult.Value);

        _initializationHandlerMock.Verify(h => h.Initialize(It.IsAny<string>(), It.IsAny<Guid>(), null), Times.Once);
        _securityGroupMetadataServiceMock.Verify(s => s.GetUserSecurityGroupsAsync(It.IsAny<string>()), Times.Never);
        _netAppArgFactoryMock.Verify(f => f.CreateDeleteFileOrFolderArg(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _netAppClientMock.Verify(c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()), Times.Never);
    }

    [Fact]
    public async Task Run_WhenFileDeletedSuccessfully_ReturnsOkObjectResultWithSuccessMessage()
    {
        // Arrange
        var operationName = "operation-123";
        var filePath = "documents/report.pdf";
        var deleteRequest = new DeleteNetAppFileOrFolderDto { Path = filePath };
        var arg = _fixture.Create<DeleteFileOrFolderArg>();
        var expectedMessage = $"Successfully deleted file {operationName}/{filePath} from bucket {_testBucketName}.";

        var securityGroups = new List<SecurityGroup>
        {
            new() {
                Id = _fixture.Create<Guid>(),
                BucketName = _testBucketName,
                DisplayName = "Test Security Group"
            }
        };

        _requestValidatorMock
            .Setup(x => x.GetJsonBody<DeleteNetAppFileOrFolderDto, DeleteNetAppFileOrFolderRequestValidator>(It.IsAny<HttpRequest>()))
            .ReturnsAsync(new ValidatableRequest<DeleteNetAppFileOrFolderDto>
            {
                IsValid = true,
                Value = deleteRequest
            });

        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(_testBearerToken))
            .ReturnsAsync(securityGroups);

        _netAppArgFactoryMock
            .Setup(f => f.CreateDeleteFileOrFolderArg(_testBearerToken, _testBucketName, operationName, $"{operationName}/{filePath}"))
            .Returns(arg);

        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(arg))
            .ReturnsAsync(expectedMessage);

        var httpRequest = HttpRequestStubHelper.CreateHttpRequestFor(deleteRequest);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        var result = await _function.Run(httpRequest, operationName, functionContext);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedMessage, okResult.Value);

        _initializationHandlerMock.Verify(h => h.Initialize(_testUsername, _testCorrelationId, null), Times.Once);
        _securityGroupMetadataServiceMock.Verify(s => s.GetUserSecurityGroupsAsync(_testBearerToken), Times.Once);
        _netAppArgFactoryMock.Verify(f => f.CreateDeleteFileOrFolderArg(_testBearerToken, _testBucketName, operationName, $"{operationName}/{filePath}"), Times.Once);
        _netAppClientMock.Verify(c => c.DeleteFileOrFolderAsync(arg), Times.Once);
    }

    [Fact]
    public async Task Run_WhenFolderDeletedSuccessfully_ReturnsOkObjectResultWithSuccessMessage()
    {
        // Arrange
        var operationName = "operation-123";
        var folderPath = "documents/reports";
        var deleteRequest = new DeleteNetAppFileOrFolderDto { Path = folderPath };
        var arg = _fixture.Create<DeleteFileOrFolderArg>();
        var expectedMessage = $"Successfully deleted {operationName}/{folderPath} from bucket {_testBucketName}.";

        var securityGroups = new List<SecurityGroup>
        {
            new() {
                Id = _fixture.Create<Guid>(),
                BucketName = _testBucketName,
                DisplayName = "Test Security Group"
            }
        };

        _requestValidatorMock
            .Setup(x => x.GetJsonBody<DeleteNetAppFileOrFolderDto, DeleteNetAppFileOrFolderRequestValidator>(It.IsAny<HttpRequest>()))
            .ReturnsAsync(new ValidatableRequest<DeleteNetAppFileOrFolderDto>
            {
                IsValid = true,
                Value = deleteRequest
            });

        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(_testBearerToken))
            .ReturnsAsync(securityGroups);

        _netAppArgFactoryMock
            .Setup(f => f.CreateDeleteFileOrFolderArg(_testBearerToken, _testBucketName, operationName, $"{operationName}/{folderPath}"))
            .Returns(arg);

        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(arg))
            .ReturnsAsync(expectedMessage);

        var httpRequest = HttpRequestStubHelper.CreateHttpRequestFor(deleteRequest);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        var result = await _function.Run(httpRequest, operationName, functionContext);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedMessage, okResult.Value);

        _initializationHandlerMock.Verify(h => h.Initialize(_testUsername, _testCorrelationId, null), Times.Once);
        _securityGroupMetadataServiceMock.Verify(s => s.GetUserSecurityGroupsAsync(_testBearerToken), Times.Once);
        _netAppArgFactoryMock.Verify(f => f.CreateDeleteFileOrFolderArg(_testBearerToken, _testBucketName, operationName, $"{operationName}/{folderPath}"), Times.Once);
        _netAppClientMock.Verify(c => c.DeleteFileOrFolderAsync(arg), Times.Once);
    }

    [Fact]
    public async Task Run_WhenNoSecurityGroupsFound_ThrowsMissingSecurityGroupException()
    {
        // Arrange
        var operationName = _fixture.Create<string>();
        var deleteRequest = _fixture.Create<DeleteNetAppFileOrFolderDto>();

        _requestValidatorMock
            .Setup(x => x.GetJsonBody<DeleteNetAppFileOrFolderDto, DeleteNetAppFileOrFolderRequestValidator>(It.IsAny<HttpRequest>()))
            .ReturnsAsync(new ValidatableRequest<DeleteNetAppFileOrFolderDto>
            {
                IsValid = true,
                Value = deleteRequest
            });

        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(_testBearerToken))
            .ThrowsAsync(new MissingSecurityGroupException("No matching security groups found for the provided IDs."));

        var httpRequest = HttpRequestStubHelper.CreateHttpRequestFor(deleteRequest);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act & Assert
        await Assert.ThrowsAsync<MissingSecurityGroupException>(() =>
            _function.Run(httpRequest, operationName, functionContext));

        _initializationHandlerMock.Verify(h => h.Initialize(_testUsername, _testCorrelationId, null), Times.Once);
        _securityGroupMetadataServiceMock.Verify(s => s.GetUserSecurityGroupsAsync(_testBearerToken), Times.Once);
        _netAppArgFactoryMock.Verify(f => f.CreateDeleteFileOrFolderArg(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _netAppClientMock.Verify(c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()), Times.Never);
    }

    [Fact]
    public async Task Run_InitializesHandlerWithCorrectParameters()
    {
        // Arrange
        var operationName = _fixture.Create<string>();
        var deleteRequest = _fixture.Create<DeleteNetAppFileOrFolderDto>();
        var arg = _fixture.Create<DeleteFileOrFolderArg>();

        var securityGroups = new List<SecurityGroup>
        {
            new() {
                Id = _fixture.Create<Guid>(),
                BucketName = _testBucketName,
                DisplayName = "Test Security Group"
            }
        };

        _requestValidatorMock
            .Setup(x => x.GetJsonBody<DeleteNetAppFileOrFolderDto, DeleteNetAppFileOrFolderRequestValidator>(It.IsAny<HttpRequest>()))
            .ReturnsAsync(new ValidatableRequest<DeleteNetAppFileOrFolderDto>
            {
                IsValid = true,
                Value = deleteRequest
            });

        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(_testBearerToken))
            .ReturnsAsync(securityGroups);

        _netAppArgFactoryMock
            .Setup(f => f.CreateDeleteFileOrFolderArg(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(arg);

        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(arg))
            .ReturnsAsync("Success");

        var httpRequest = HttpRequestStubHelper.CreateHttpRequestFor(deleteRequest);
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
        var operationName = "operation-123";
        var filePath = "file.pdf";
        var firstBucketName = "first-bucket";
        var secondBucketName = "second-bucket";
        var deleteRequest = new DeleteNetAppFileOrFolderDto { Path = filePath };
        var arg = _fixture.Create<DeleteFileOrFolderArg>();

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

        _requestValidatorMock
            .Setup(x => x.GetJsonBody<DeleteNetAppFileOrFolderDto, DeleteNetAppFileOrFolderRequestValidator>(It.IsAny<HttpRequest>()))
            .ReturnsAsync(new ValidatableRequest<DeleteNetAppFileOrFolderDto>
            {
                IsValid = true,
                Value = deleteRequest
            });

        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(_testBearerToken))
            .ReturnsAsync(securityGroups);

        _netAppArgFactoryMock
            .Setup(f => f.CreateDeleteFileOrFolderArg(_testBearerToken, firstBucketName, operationName, $"{operationName}/{filePath}"))
            .Returns(arg);

        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(arg))
            .ReturnsAsync("Success");

        var httpRequest = HttpRequestStubHelper.CreateHttpRequestFor(deleteRequest);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        await _function.Run(httpRequest, operationName, functionContext);

        // Assert
        _netAppArgFactoryMock.Verify(f => f.CreateDeleteFileOrFolderArg(_testBearerToken, firstBucketName, operationName, $"{operationName}/{filePath}"), Times.Once);
    }

    [Fact]
    public async Task Run_CombinesOperationNameWithPathCorrectly()
    {
        // Arrange
        var operationName = "operation-123";
        var filePath = "documents/report.pdf";
        var expectedFullPath = $"{operationName}/{filePath}";
        var deleteRequest = new DeleteNetAppFileOrFolderDto { Path = filePath };
        var arg = _fixture.Create<DeleteFileOrFolderArg>();

        var securityGroups = new List<SecurityGroup>
        {
            new() {
                Id = _fixture.Create<Guid>(),
                BucketName = _testBucketName,
                DisplayName = "Test Security Group"
            }
        };

        _requestValidatorMock
            .Setup(x => x.GetJsonBody<DeleteNetAppFileOrFolderDto, DeleteNetAppFileOrFolderRequestValidator>(It.IsAny<HttpRequest>()))
            .ReturnsAsync(new ValidatableRequest<DeleteNetAppFileOrFolderDto>
            {
                IsValid = true,
                Value = deleteRequest
            });

        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(_testBearerToken))
            .ReturnsAsync(securityGroups);

        _netAppArgFactoryMock
            .Setup(f => f.CreateDeleteFileOrFolderArg(_testBearerToken, _testBucketName, operationName, expectedFullPath))
            .Returns(arg);

        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(arg))
            .ReturnsAsync("Success");

        var httpRequest = HttpRequestStubHelper.CreateHttpRequestFor(deleteRequest);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        await _function.Run(httpRequest, operationName, functionContext);

        // Assert
        _netAppArgFactoryMock.Verify(f => f.CreateDeleteFileOrFolderArg(
            _testBearerToken,
            _testBucketName,
            operationName,
            expectedFullPath), Times.Once);
    }
}
