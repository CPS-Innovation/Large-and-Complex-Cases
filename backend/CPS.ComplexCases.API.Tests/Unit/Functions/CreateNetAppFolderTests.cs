using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AutoFixture;
using CPS.ComplexCases.ActivityLog.Enums;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.API.Domain.Models;
using CPS.ComplexCases.API.Exceptions;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models;
using CPS.ComplexCases.Data.Models.Requests;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.Dto;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions;

public class CreateNetAppFolderTests
{
    private readonly Mock<ILogger<CreateNetAppFolder>> _loggerMock;
    private readonly Mock<INetAppClient> _netAppClientMock;
    private readonly Mock<INetAppArgFactory> _netAppArgFactoryMock;
    private readonly Mock<IActivityLogService> _activityLogServiceMock;
    private readonly Mock<IRequestValidator> _requestValidatorMock;
    private readonly Mock<ISecurityGroupMetadataService> _securityGroupMetadataServiceMock;
    private readonly Mock<IInitializationHandler> _initializationHandlerMock;
    private readonly CreateNetAppFolder _function;
    private readonly Fixture _fixture;
    private readonly string _testBearerToken;
    private readonly string _testBucketName;
    private readonly Guid _testCorrelationId;
    private readonly string _testUsername;
    private readonly string _testCmsAuthValues;
    private readonly List<SecurityGroup> _defaultSecurityGroups;

    public CreateNetAppFolderTests()
    {
        _loggerMock = new Mock<ILogger<CreateNetAppFolder>>();
        _netAppClientMock = new Mock<INetAppClient>();
        _netAppArgFactoryMock = new Mock<INetAppArgFactory>();
        _activityLogServiceMock = new Mock<IActivityLogService>();
        _requestValidatorMock = new Mock<IRequestValidator>();
        _securityGroupMetadataServiceMock = new Mock<ISecurityGroupMetadataService>();
        _initializationHandlerMock = new Mock<IInitializationHandler>();

        _fixture = new Fixture();
        _testBearerToken = _fixture.Create<string>();
        _testBucketName = _fixture.Create<string>();
        _testCorrelationId = _fixture.Create<Guid>();
        _testUsername = _fixture.Create<string>();
        _testCmsAuthValues = _fixture.Create<string>();

        _defaultSecurityGroups =
        [
            new() {
                Id = _fixture.Create<Guid>(),
                BucketName = _testBucketName,
                DisplayName = "Test Security Group"
            }
        ];

        _function = new CreateNetAppFolder(
            _loggerMock.Object,
            _netAppClientMock.Object,
            _netAppArgFactoryMock.Object,
            _activityLogServiceMock.Object,
            _requestValidatorMock.Object,
            _securityGroupMetadataServiceMock.Object,
            _initializationHandlerMock.Object);
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void SetupValidRequest(CreateNetAppFolderDto dto)
    {
        _requestValidatorMock
            .Setup(x => x.GetJsonBody<CreateNetAppFolderDto, CreateNetAppFolderRequestValidator>(It.IsAny<HttpRequest>()))
            .ReturnsAsync(new ValidatableRequest<CreateNetAppFolderDto> { IsValid = true, Value = dto });
    }

    private void SetupNoExistingFolders(string folderPath)
    {
        var listArg = _fixture.Create<ListFoldersInBucketArg>();
        _netAppArgFactoryMock
            .Setup(f => f.CreateListFoldersInBucketArg(_testBearerToken, _testBucketName, null, null, null, folderPath))
            .Returns(listArg);

        _netAppClientMock
            .Setup(c => c.ListFoldersInBucketAsync(listArg))
            .ReturnsAsync(new ListNetAppObjectsDto
            {
                Data = new ListNetAppDataDto
                {
                    BucketName = _testBucketName,
                    FolderData = [],
                    FileData = []
                },
                Pagination = new PaginationDto()
            });
    }

    // -------------------------------------------------------------------------
    // Validation
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Run_WhenRequestBodyIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        var validationErrors = _fixture.CreateMany<string>(2).ToList();

        _requestValidatorMock
            .Setup(x => x.GetJsonBody<CreateNetAppFolderDto, CreateNetAppFolderRequestValidator>(It.IsAny<HttpRequest>()))
            .ReturnsAsync(new ValidatableRequest<CreateNetAppFolderDto>
            {
                IsValid = false,
                ValidationErrors = validationErrors,
                Value = new CreateNetAppFolderDto { Path = "op/folder", CaseId = 1 }
            });

        var httpRequest = new DefaultHttpContext().Request;
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errors = Assert.IsAssignableFrom<IEnumerable<string>>(badRequestResult.Value);
        Assert.Equal(validationErrors, errors);

        _initializationHandlerMock.Verify(h => h.Initialize(_testUsername, _testCorrelationId, null), Times.Once);
        _securityGroupMetadataServiceMock.Verify(s => s.GetUserSecurityGroupsAsync(It.IsAny<string>()), Times.Never);
        _netAppClientMock.Verify(c => c.ListFoldersInBucketAsync(It.IsAny<ListFoldersInBucketArg>()), Times.Never);
        _netAppClientMock.Verify(c => c.CreateFolderAsync(It.IsAny<CreateFolderArg>()), Times.Never);
    }

    // -------------------------------------------------------------------------
    // Duplicate check
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Run_WhenFolderAlreadyExists_ReturnsConflict()
    {
        // Arrange
        var folderPath = "operation-123/my-folder";
        var dto = new CreateNetAppFolderDto { Path = folderPath, CaseId = 42 };
        SetupValidRequest(dto);

        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(_testBearerToken))
            .ReturnsAsync(_defaultSecurityGroups);

        var listArg = _fixture.Create<ListFoldersInBucketArg>();
        _netAppArgFactoryMock
            .Setup(f => f.CreateListFoldersInBucketArg(_testBearerToken, _testBucketName, null, null, null, folderPath))
            .Returns(listArg);

        _netAppClientMock
            .Setup(c => c.ListFoldersInBucketAsync(listArg))
            .ReturnsAsync(new ListNetAppObjectsDto
            {
                Data = new ListNetAppDataDto
                {
                    BucketName = _testBucketName,
                    FolderData = [new ListNetAppFolderDataDto { Path = folderPath + "/" }],
                    FileData = []
                },
                Pagination = new PaginationDto()
            });

        var httpRequest = HttpRequestStubHelper.CreateHttpRequestFor(dto);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        Assert.Contains(folderPath, conflictResult.Value!.ToString());

        _netAppClientMock.Verify(c => c.CreateFolderAsync(It.IsAny<CreateFolderArg>()), Times.Never);
        _activityLogServiceMock.Verify(
            a => a.CreateActivityLogAsync(It.IsAny<ActionType>(), It.IsAny<ResourceType>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.Text.Json.JsonDocument>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_WhenListFoldersReturnsNull_DoesNotReturnConflict()
    {
        // Arrange
        var folderPath = "operation-123/my-folder";
        var dto = new CreateNetAppFolderDto { Path = folderPath, CaseId = 42 };
        SetupValidRequest(dto);

        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(_testBearerToken))
            .ReturnsAsync(_defaultSecurityGroups);

        var listArg = _fixture.Create<ListFoldersInBucketArg>();
        _netAppArgFactoryMock
            .Setup(f => f.CreateListFoldersInBucketArg(_testBearerToken, _testBucketName, null, null, null, folderPath))
            .Returns(listArg);

        _netAppClientMock
            .Setup(c => c.ListFoldersInBucketAsync(listArg))
            .ReturnsAsync((ListNetAppObjectsDto?)null);

        var createArg = _fixture.Create<CreateFolderArg>();
        _netAppArgFactoryMock
            .Setup(f => f.CreateCreateFolderArg(_testBearerToken, _testBucketName, folderPath))
            .Returns(createArg);

        _netAppClientMock
            .Setup(c => c.CreateFolderAsync(createArg))
            .ReturnsAsync(true);

        var httpRequest = HttpRequestStubHelper.CreateHttpRequestFor(dto);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        _netAppClientMock.Verify(c => c.CreateFolderAsync(createArg), Times.Once);
    }

    // -------------------------------------------------------------------------
    // Successful creation
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Run_WhenFolderCreatedSuccessfully_ReturnsOkObjectResult()
    {
        // Arrange
        var folderPath = "operation-123/my-folder";
        var caseId = 99;
        var dto = new CreateNetAppFolderDto { Path = folderPath, CaseId = caseId };
        SetupValidRequest(dto);

        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(_testBearerToken))
            .ReturnsAsync(_defaultSecurityGroups);

        SetupNoExistingFolders(folderPath);

        var createArg = _fixture.Create<CreateFolderArg>();
        _netAppArgFactoryMock
            .Setup(f => f.CreateCreateFolderArg(_testBearerToken, _testBucketName, folderPath))
            .Returns(createArg);

        _netAppClientMock
            .Setup(c => c.CreateFolderAsync(createArg))
            .ReturnsAsync(true);

        var httpRequest = HttpRequestStubHelper.CreateHttpRequestFor(dto);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.True((bool)okResult.Value!);

        _initializationHandlerMock.Verify(h => h.Initialize(_testUsername, _testCorrelationId, null), Times.Once);
        _securityGroupMetadataServiceMock.Verify(s => s.GetUserSecurityGroupsAsync(_testBearerToken), Times.Once);
        _netAppArgFactoryMock.Verify(f => f.CreateCreateFolderArg(_testBearerToken, _testBucketName, folderPath), Times.Once);
        _netAppClientMock.Verify(c => c.CreateFolderAsync(createArg), Times.Once);
    }

    [Fact]
    public async Task Run_WhenFolderCreatedSuccessfully_LogsActivityWithCorrectParameters()
    {
        // Arrange
        var folderPath = "operation-123/my-folder";
        var expectedFolderName = "my-folder";
        var caseId = 99;
        var dto = new CreateNetAppFolderDto { Path = folderPath, CaseId = caseId };
        SetupValidRequest(dto);

        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(_testBearerToken))
            .ReturnsAsync(_defaultSecurityGroups);

        SetupNoExistingFolders(folderPath);

        var createArg = _fixture.Create<CreateFolderArg>();
        _netAppArgFactoryMock
            .Setup(f => f.CreateCreateFolderArg(_testBearerToken, _testBucketName, folderPath))
            .Returns(createArg);

        _netAppClientMock
            .Setup(c => c.CreateFolderAsync(createArg))
            .ReturnsAsync(true);

        var httpRequest = HttpRequestStubHelper.CreateHttpRequestFor(dto);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        await _function.Run(httpRequest, functionContext);

        // Assert
        _activityLogServiceMock.Verify(
            a => a.CreateActivityLogAsync(
                ActionType.FolderCreated,
                ResourceType.NetAppFolder,
                caseId,
                folderPath,
                expectedFolderName,
                _testUsername,
                null),
            Times.Once);
    }

    [Fact]
    public async Task Run_WhenPathHasNoSlash_UsesFolderPathAsFolderName()
    {
        // Arrange
        var folderPath = "operation-123";
        var caseId = 7;
        var dto = new CreateNetAppFolderDto { Path = folderPath, CaseId = caseId };
        SetupValidRequest(dto);

        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(_testBearerToken))
            .ReturnsAsync(_defaultSecurityGroups);

        SetupNoExistingFolders(folderPath);

        var createArg = _fixture.Create<CreateFolderArg>();
        _netAppArgFactoryMock
            .Setup(f => f.CreateCreateFolderArg(_testBearerToken, _testBucketName, folderPath))
            .Returns(createArg);

        _netAppClientMock
            .Setup(c => c.CreateFolderAsync(createArg))
            .ReturnsAsync(true);

        var httpRequest = HttpRequestStubHelper.CreateHttpRequestFor(dto);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        await _function.Run(httpRequest, functionContext);

        // Assert: folder name should equal the full path when no slash is present
        _activityLogServiceMock.Verify(
            a => a.CreateActivityLogAsync(
                ActionType.FolderCreated,
                ResourceType.NetAppFolder,
                caseId,
                folderPath,
                folderPath,
                _testUsername,
                null),
            Times.Once);
    }

    // -------------------------------------------------------------------------
    // Creation failure
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Run_WhenFolderCreationFails_ReturnsInternalServerError()
    {
        // Arrange
        var folderPath = "operation-123/my-folder";
        var dto = new CreateNetAppFolderDto { Path = folderPath, CaseId = 1 };
        SetupValidRequest(dto);

        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(_testBearerToken))
            .ReturnsAsync(_defaultSecurityGroups);

        SetupNoExistingFolders(folderPath);

        var createArg = _fixture.Create<CreateFolderArg>();
        _netAppArgFactoryMock
            .Setup(f => f.CreateCreateFolderArg(_testBearerToken, _testBucketName, folderPath))
            .Returns(createArg);

        _netAppClientMock
            .Setup(c => c.CreateFolderAsync(createArg))
            .ReturnsAsync(false);

        var httpRequest = HttpRequestStubHelper.CreateHttpRequestFor(dto);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert
        var statusResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal((int)HttpStatusCode.InternalServerError, statusResult.StatusCode);

        _activityLogServiceMock.Verify(
            a => a.CreateActivityLogAsync(It.IsAny<ActionType>(), It.IsAny<ResourceType>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.Text.Json.JsonDocument>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_WhenActivityLogThrows_StillReturnsOk()
    {
        // Arrange
        var folderPath = "operation-123/my-folder";
        var caseId = 99;
        var dto = new CreateNetAppFolderDto { Path = folderPath, CaseId = caseId };
        SetupValidRequest(dto);

        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(_testBearerToken))
            .ReturnsAsync(_defaultSecurityGroups);

        SetupNoExistingFolders(folderPath);

        var createArg = _fixture.Create<CreateFolderArg>();
        _netAppArgFactoryMock
            .Setup(f => f.CreateCreateFolderArg(_testBearerToken, _testBucketName, folderPath))
            .Returns(createArg);

        _netAppClientMock
            .Setup(c => c.CreateFolderAsync(createArg))
            .ReturnsAsync(true);

        _activityLogServiceMock
            .Setup(a => a.CreateActivityLogAsync(It.IsAny<ActionType>(), It.IsAny<ResourceType>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<System.Text.Json.JsonDocument>()))
            .ThrowsAsync(new Exception("Activity log unavailable"));

        var httpRequest = HttpRequestStubHelper.CreateHttpRequestFor(dto);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        var result = await _function.Run(httpRequest, functionContext);

        // Assert — folder creation succeeded; logging failure must not surface as an error
        Assert.IsType<OkObjectResult>(result);
    }

    // -------------------------------------------------------------------------
    // Path normalisation
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Run_StripsTrailingSlashFromPath()
    {
        // Arrange — path supplied with trailing slash should be normalised
        var folderPathWithSlash = "operation-123/my-folder/";
        var normalised = "operation-123/my-folder";
        var dto = new CreateNetAppFolderDto { Path = folderPathWithSlash, CaseId = 5 };
        SetupValidRequest(dto);

        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(_testBearerToken))
            .ReturnsAsync(_defaultSecurityGroups);

        SetupNoExistingFolders(normalised);

        var createArg = _fixture.Create<CreateFolderArg>();
        _netAppArgFactoryMock
            .Setup(f => f.CreateCreateFolderArg(_testBearerToken, _testBucketName, normalised))
            .Returns(createArg);

        _netAppClientMock
            .Setup(c => c.CreateFolderAsync(createArg))
            .ReturnsAsync(true);

        var httpRequest = HttpRequestStubHelper.CreateHttpRequestFor(dto);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        await _function.Run(httpRequest, functionContext);

        // Assert — factory called with the normalised (no trailing slash) path
        _netAppArgFactoryMock.Verify(
            f => f.CreateCreateFolderArg(_testBearerToken, _testBucketName, normalised),
            Times.Once);
        _netAppArgFactoryMock.Verify(
            f => f.CreateCreateFolderArg(It.IsAny<string>(), It.IsAny<string>(), folderPathWithSlash),
            Times.Never);
    }

    // -------------------------------------------------------------------------
    // Security groups
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Run_WhenNoSecurityGroupsFound_ThrowsMissingSecurityGroupException()
    {
        // Arrange
        var dto = new CreateNetAppFolderDto { Path = "op/folder", CaseId = 1 };
        SetupValidRequest(dto);

        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(_testBearerToken))
            .ThrowsAsync(new MissingSecurityGroupException("No matching security groups found for the provided IDs."));

        var httpRequest = HttpRequestStubHelper.CreateHttpRequestFor(dto);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act & Assert
        await Assert.ThrowsAsync<MissingSecurityGroupException>(() =>
            _function.Run(httpRequest, functionContext));

        _initializationHandlerMock.Verify(h => h.Initialize(_testUsername, _testCorrelationId, null), Times.Once);
        _securityGroupMetadataServiceMock.Verify(s => s.GetUserSecurityGroupsAsync(_testBearerToken), Times.Once);
        _netAppClientMock.Verify(c => c.ListFoldersInBucketAsync(It.IsAny<ListFoldersInBucketArg>()), Times.Never);
        _netAppClientMock.Verify(c => c.CreateFolderAsync(It.IsAny<CreateFolderArg>()), Times.Never);
    }

    [Fact]
    public async Task Run_UsesFirstSecurityGroupBucketName()
    {
        // Arrange
        var firstBucketName = "first-bucket";
        var secondBucketName = "second-bucket";
        var folderPath = "operation-123/my-folder";
        var dto = new CreateNetAppFolderDto { Path = folderPath, CaseId = 1 };
        SetupValidRequest(dto);

        var securityGroups = new List<SecurityGroup>
        {
            new() { Id = _fixture.Create<Guid>(), BucketName = firstBucketName, DisplayName = "First" },
            new() { Id = _fixture.Create<Guid>(), BucketName = secondBucketName, DisplayName = "Second" }
        };
        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(_testBearerToken))
            .ReturnsAsync(securityGroups);

        var listArg = _fixture.Create<ListFoldersInBucketArg>();
        _netAppArgFactoryMock
            .Setup(f => f.CreateListFoldersInBucketArg(_testBearerToken, firstBucketName, null, null, null, folderPath))
            .Returns(listArg);
        _netAppClientMock
            .Setup(c => c.ListFoldersInBucketAsync(listArg))
            .ReturnsAsync(new ListNetAppObjectsDto
            {
                Data = new ListNetAppDataDto { BucketName = firstBucketName, FolderData = [], FileData = [] },
                Pagination = new PaginationDto()
            });

        var createArg = _fixture.Create<CreateFolderArg>();
        _netAppArgFactoryMock
            .Setup(f => f.CreateCreateFolderArg(_testBearerToken, firstBucketName, folderPath))
            .Returns(createArg);
        _netAppClientMock
            .Setup(c => c.CreateFolderAsync(createArg))
            .ReturnsAsync(true);

        var httpRequest = HttpRequestStubHelper.CreateHttpRequestFor(dto);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

        // Act
        await _function.Run(httpRequest, functionContext);

        // Assert
        _netAppArgFactoryMock.Verify(f => f.CreateCreateFolderArg(_testBearerToken, firstBucketName, folderPath), Times.Once);
        _netAppArgFactoryMock.Verify(f => f.CreateCreateFolderArg(It.IsAny<string>(), secondBucketName, It.IsAny<string>()), Times.Never);
    }
}
