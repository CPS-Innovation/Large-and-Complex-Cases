using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AutoFixture;
using CPS.ComplexCases.ActivityLog.Enums;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.Api.Functions;
using CPS.ComplexCases.API.Clients.FileTransfer;
using CPS.ComplexCases.API.Domain.Models;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.Data.Models.Requests;
using CPS.ComplexCases.DDEI.Client;
using CPS.ComplexCases.DDEI.Factories;
using CPS.ComplexCases.DDEI.Models.Args;
using CPS.ComplexCases.DDEI.Models.Dto;
using CPS.ComplexCases.DDEI.Services;
using CPS.ComplexCases.NetApp.Models.Dto;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions;

public class ProvisionNetAppFoldersTests
{
    private readonly Mock<ILogger<ProvisionNetAppFolders>> _loggerMock;
    private readonly Mock<IDdeiClient> _ddeiClientMock;
    private readonly Mock<IDdeiArgFactory> _ddeiArgFactoryMock;
    private readonly Mock<ICaseNamingService> _caseNamingServiceMock;
    private readonly Mock<IFileTransferClient> _fileTransferClientMock;
    private readonly Mock<IRequestValidator> _requestValidatorMock;
    private readonly Mock<ISecurityGroupMetadataService> _securityGroupMetadataServiceMock;
    private readonly Mock<ICaseMetadataService> _caseMetadataServiceMock;
    private readonly Mock<IActivityLogService> _activityLogServiceMock;
    private readonly Mock<IInitializationHandler> _initializationHandlerMock;
    private readonly ProvisionNetAppFolders _function;
    private readonly Fixture _fixture;

    private readonly Guid _correlationId;
    private readonly string _username;
    private readonly string _cmsAuthValues;
    private readonly string _bearerToken;
    private readonly int _caseId;
    private readonly string _bucketName;
    private readonly string _caseName;
    private readonly string _operationName;
    private readonly CaseNameDto _caseNameDto;

    public ProvisionNetAppFoldersTests()
    {
        _fixture = new Fixture();
        _loggerMock = new Mock<ILogger<ProvisionNetAppFolders>>();
        _ddeiClientMock = new Mock<IDdeiClient>();
        _ddeiArgFactoryMock = new Mock<IDdeiArgFactory>();
        _caseNamingServiceMock = new Mock<ICaseNamingService>();
        _fileTransferClientMock = new Mock<IFileTransferClient>();
        _requestValidatorMock = new Mock<IRequestValidator>();
        _securityGroupMetadataServiceMock = new Mock<ISecurityGroupMetadataService>();
        _caseMetadataServiceMock = new Mock<ICaseMetadataService>();
        _activityLogServiceMock = new Mock<IActivityLogService>();
        _initializationHandlerMock = new Mock<IInitializationHandler>();

        _correlationId = _fixture.Create<Guid>();
        _username = _fixture.Create<string>();
        _cmsAuthValues = _fixture.Create<string>();
        _bearerToken = _fixture.Create<string>();
        _caseId = Math.Abs(_fixture.Create<int>()) + 1; // ensure > 0
        _bucketName = _fixture.Create<string>();
        _caseName = _fixture.Create<string>();
        _operationName = _fixture.Create<string>();
        _caseNameDto = new CaseNameDto { CaseName = _caseName, OperationName = _operationName };

        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(It.IsAny<string>()))
            .ReturnsAsync([
                new SecurityGroup
                {
                    Id = _fixture.Create<Guid>(),
                    BucketName = _bucketName,
                    VolumeUuid = _fixture.Create<Guid>(),
                    DisplayName = "Test Security Group"
                }
            ]);

        _function = new ProvisionNetAppFolders(
            _loggerMock.Object,
            _ddeiClientMock.Object,
            _ddeiArgFactoryMock.Object,
            _caseNamingServiceMock.Object,
            _fileTransferClientMock.Object,
            _requestValidatorMock.Object,
            _securityGroupMetadataServiceMock.Object,
            _caseMetadataServiceMock.Object,
            _activityLogServiceMock.Object,
            _initializationHandlerMock.Object);
    }

    [Fact]
    public async Task Run_WhenCaseIdIsZero_ReturnsBadRequest()
    {
        // Arrange
        var request = HttpRequestStubHelper.CreateHttpRequest(_correlationId);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _cmsAuthValues, _username, _bearerToken);

        // Act
        var result = await _function.Run(request, functionContext, 0);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid caseId parameter.", badRequest.Value);
    }

    [Fact]
    public async Task Run_WhenCaseIdIsNegative_ReturnsBadRequest()
    {
        // Arrange
        var request = HttpRequestStubHelper.CreateHttpRequest(_correlationId);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _cmsAuthValues, _username, _bearerToken);

        // Act
        var result = await _function.Run(request, functionContext, -1);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Run_WhenRequestBodyIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        var validationErrors = _fixture.CreateMany<string>(2).ToList();
        var dto = _fixture.Create<ProvisionNetAppFoldersDto>();

        _requestValidatorMock
            .Setup(v => v.GetJsonBody<ProvisionNetAppFoldersDto, ProvisionNetAppFoldersRequestValidator>(It.IsAny<HttpRequest>()))
            .ReturnsAsync(new ValidatableRequest<ProvisionNetAppFoldersDto>
            {
                IsValid = false,
                ValidationErrors = validationErrors,
                Value = dto
            });

        var request = HttpRequestStubHelper.CreateHttpRequestFor(dto);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _cmsAuthValues, _username, _bearerToken);

        // Act
        var result = await _function.Run(request, functionContext, _caseId);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var errors = Assert.IsAssignableFrom<IEnumerable<string>>(badRequest.Value);
        Assert.Equal(validationErrors, errors);
    }

    [Fact]
    public async Task Run_WhenCaseNotFoundInCms_ReturnsNotFound()
    {
        // Arrange
        var dto = _fixture.Create<ProvisionNetAppFoldersDto>();
        var cmsArg = _fixture.Create<DdeiCaseIdArgDto>();

        SetupValidRequest(dto, cmsArg);

        _ddeiClientMock
            .Setup(c => c.GetCaseAsync(cmsArg))
            .ReturnsAsync((CaseDto?)null!);

        var request = HttpRequestStubHelper.CreateHttpRequestFor(dto);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _cmsAuthValues, _username, _bearerToken);

        // Act
        var result = await _function.Run(request, functionContext, _caseId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Run_WhenCaseAlreadyHasNetAppFolder_ReturnsConflict()
    {
        // Arrange
        var dto = _fixture.Create<ProvisionNetAppFoldersDto>();
        var cmsArg = _fixture.Create<DdeiCaseIdArgDto>();
        var caseResponse = _fixture.Create<CaseDto>();
        var caseMetadata = new CaseMetadata { NetappFolderPath = "some/existing/path/", CaseId = _caseId };

        SetupValidRequest(dto, cmsArg);

        _ddeiClientMock
            .Setup(c => c.GetCaseAsync(cmsArg))
            .ReturnsAsync(caseResponse);

        _caseMetadataServiceMock
            .Setup(s => s.GetCaseMetadataForCaseIdAsync(_caseId))
            .ReturnsAsync(caseMetadata);

        var request = HttpRequestStubHelper.CreateHttpRequestFor(dto);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _cmsAuthValues, _username, _bearerToken);

        // Act
        var result = await _function.Run(request, functionContext, _caseId);

        // Assert
        var conflict = Assert.IsType<ConflictObjectResult>(result);
        Assert.Contains(_caseId.ToString(), conflict.Value?.ToString());
    }

    [Fact]
    public async Task Run_WhenActiveTransferExists_ReturnsConflict()
    {
        // Arrange
        var dto = _fixture.Create<ProvisionNetAppFoldersDto>();
        var cmsArg = _fixture.Create<DdeiCaseIdArgDto>();
        var caseResponse = _fixture.Create<CaseDto>();
        var caseMetadata = new CaseMetadata { ActiveTransferId = Guid.NewGuid(), CaseId = _caseId };

        SetupValidRequest(dto, cmsArg);

        _ddeiClientMock
            .Setup(c => c.GetCaseAsync(cmsArg))
            .ReturnsAsync(caseResponse);

        _caseMetadataServiceMock
            .Setup(s => s.GetCaseMetadataForCaseIdAsync(_caseId))
            .ReturnsAsync(caseMetadata);

        var request = HttpRequestStubHelper.CreateHttpRequestFor(dto);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _cmsAuthValues, _username, _bearerToken);

        // Act
        var result = await _function.Run(request, functionContext, _caseId);

        // Assert
        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Run_WhenRequestIsValid_CallsProvisionNetAppFolders()
    {
        // Arrange
        var dto = _fixture.Create<ProvisionNetAppFoldersDto>();
        var cmsArg = _fixture.Create<DdeiCaseIdArgDto>();
        var caseResponse = _fixture.Create<CaseDto>();

        SetupValidRequest(dto, cmsArg);
        SetupSuccessfulTransfer(caseResponse, "Completed");

        var request = HttpRequestStubHelper.CreateHttpRequestFor(dto);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _cmsAuthValues, _username, _bearerToken);

        // Act
        await _function.Run(request, functionContext, _caseId);

        // Assert
        _fileTransferClientMock.Verify(c => c.ProvisionNetAppFoldersAsync(
            It.Is<ProvisionNetAppFoldersRequest>(r =>
                r.CaseId == _caseId &&
                r.TemplateName == dto.TemplateFolderPath &&
                r.BucketName == _bucketName &&
                r.BearerToken == _bearerToken &&
                r.UserName == _username),
            _correlationId), Times.Once);
    }

    [Fact]
    public async Task Run_WhenRequestIsValid_CreatesNetAppConnectionMetadata()
    {
        // Arrange
        var dto = _fixture.Create<ProvisionNetAppFoldersDto>();
        var cmsArg = _fixture.Create<DdeiCaseIdArgDto>();
        var caseResponse = _fixture.Create<CaseDto>();
        var expectedFolderPath = $"{_caseName}/";

        SetupValidRequest(dto, cmsArg);
        SetupSuccessfulTransfer(caseResponse, "Completed");

        var request = HttpRequestStubHelper.CreateHttpRequestFor(dto);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _cmsAuthValues, _username, _bearerToken);

        // Act
        await _function.Run(request, functionContext, _caseId);

        // Assert
        _caseMetadataServiceMock.Verify(s => s.CreateNetAppConnectionAsync(
            It.Is<CreateNetAppConnectionDto>(c =>
                c.CaseId == _caseId &&
                c.NetAppFolderPath == expectedFolderPath)), Times.Once);
    }

    [Fact]
    public async Task Run_WhenRequestIsValid_CreatesActivityLog()
    {
        // Arrange
        var dto = _fixture.Create<ProvisionNetAppFoldersDto>();
        var cmsArg = _fixture.Create<DdeiCaseIdArgDto>();
        var caseResponse = _fixture.Create<CaseDto>();

        SetupValidRequest(dto, cmsArg);
        SetupSuccessfulTransfer(caseResponse, "Completed");

        var request = HttpRequestStubHelper.CreateHttpRequestFor(dto);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _cmsAuthValues, _username, _bearerToken);

        // Act
        await _function.Run(request, functionContext, _caseId);

        // Assert
        _activityLogServiceMock.Verify(s => s.CreateActivityLogAsync(
            ActionType.ConnectionToNetApp,
            ResourceType.StorageConnection,
            _caseId,
            _caseName,
            _caseName,
            _username,
            null), Times.Once);
    }

    [Fact]
    public async Task Run_WhenRequestIsValid_ReturnsOkWithCaseName()
    {
        // Arrange
        var dto = _fixture.Create<ProvisionNetAppFoldersDto>();
        var cmsArg = _fixture.Create<DdeiCaseIdArgDto>();
        var caseResponse = _fixture.Create<CaseDto>();
        var expectedFolderPath = $"{_caseNameDto.CaseName}/";

        SetupValidRequest(dto, cmsArg);
        SetupSuccessfulTransfer(caseResponse, "Completed");

        var request = HttpRequestStubHelper.CreateHttpRequestFor(dto);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _cmsAuthValues, _username, _bearerToken);

        // Act
        var result = await _function.Run(request, functionContext, _caseId);

        // Assert — the gateway returns the CaseName (not the folder path)
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedFolderPath, ok.Value);
    }

    [Fact]
    public async Task Run_WhenCaseMetadataIsNullAndNoActiveTransfer_ProceedsToProvisioning()
    {
        // Arrange
        var dto = _fixture.Create<ProvisionNetAppFoldersDto>();
        var cmsArg = _fixture.Create<DdeiCaseIdArgDto>();
        var caseResponse = _fixture.Create<CaseDto>();

        SetupValidRequest(dto, cmsArg);
        SetupSuccessfulTransfer(caseResponse, "Completed");

        var request = HttpRequestStubHelper.CreateHttpRequestFor(dto);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _cmsAuthValues, _username, _bearerToken);

        // Act
        var result = await _function.Run(request, functionContext, _caseId);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        _fileTransferClientMock.Verify(c => c.ProvisionNetAppFoldersAsync(It.IsAny<ProvisionNetAppFoldersRequest>(), _correlationId), Times.Once);
    }

    [Fact]
    public async Task Run_WhenOrchestrationReturnsFailed_Returns500()
    {
        // Arrange
        var dto = _fixture.Create<ProvisionNetAppFoldersDto>();
        var cmsArg = _fixture.Create<DdeiCaseIdArgDto>();
        var caseResponse = _fixture.Create<CaseDto>();

        SetupValidRequest(dto, cmsArg);
        SetupSuccessfulTransfer(caseResponse, "Failed");

        var request = HttpRequestStubHelper.CreateHttpRequestFor(dto);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _cmsAuthValues, _username, _bearerToken);

        // Act
        var result = await _function.Run(request, functionContext, _caseId);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        _caseMetadataServiceMock.Verify(s => s.CreateNetAppConnectionAsync(It.IsAny<CreateNetAppConnectionDto>()), Times.Never);
        _activityLogServiceMock.Verify(s => s.CreateActivityLogAsync(
            It.IsAny<ActionType>(), It.IsAny<ResourceType>(),
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), null), Times.Never);
    }

    [Fact]
    public async Task Run_WhenOrchestrationReturnsPartiallyCompleted_ReturnsBadRequest()
    {
        // Arrange
        var dto = _fixture.Create<ProvisionNetAppFoldersDto>();
        var cmsArg = _fixture.Create<DdeiCaseIdArgDto>();
        var caseResponse = _fixture.Create<CaseDto>();

        SetupValidRequest(dto, cmsArg);
        SetupSuccessfulTransfer(caseResponse, "PartiallyCompleted");

        var request = HttpRequestStubHelper.CreateHttpRequestFor(dto);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _cmsAuthValues, _username, _bearerToken);

        // Act
        var result = await _function.Run(request, functionContext, _caseId);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);
        _caseMetadataServiceMock.Verify(s => s.CreateNetAppConnectionAsync(It.IsAny<CreateNetAppConnectionDto>()), Times.Never);
        _activityLogServiceMock.Verify(s => s.CreateActivityLogAsync(
            It.IsAny<ActionType>(), It.IsAny<ResourceType>(),
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), null), Times.Never);
    }

    [Fact]
    public async Task Run_WhenFileTransferReturns200AndStatusIsInitiallyInProgress_PollsUntilCompleted()
    {
        var dto = _fixture.Create<ProvisionNetAppFoldersDto>();
        var cmsArg = _fixture.Create<DdeiCaseIdArgDto>();
        var caseResponse = _fixture.Create<CaseDto>();
        var transferId = Guid.NewGuid();

        SetupValidRequest(dto, cmsArg);

        _ddeiClientMock.Setup(c => c.GetCaseAsync(cmsArg)).ReturnsAsync(caseResponse);
        _caseMetadataServiceMock.Setup(s => s.GetCaseMetadataForCaseIdAsync(_caseId)).ReturnsAsync((CaseMetadata?)null);
        _caseNamingServiceMock.Setup(s => s.GenerateCaseName(caseResponse)).ReturnsAsync(_caseNameDto);

        _fileTransferClientMock
            .Setup(c => c.ProvisionNetAppFoldersAsync(It.IsAny<ProvisionNetAppFoldersRequest>(), _correlationId))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new { id = transferId }), Encoding.UTF8, "application/json")
            });

        _fileTransferClientMock
            .SetupSequence(c => c.GetFileTransferStatusAsync(transferId.ToString(), _correlationId, null))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new { status = "InProgress" }), Encoding.UTF8, "application/json")
            })
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new { status = "Completed" }), Encoding.UTF8, "application/json")
            });

        var request = HttpRequestStubHelper.CreateHttpRequestFor(dto);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _cmsAuthValues, _username, _bearerToken);

        // Act
        var result = await _function.Run(request, functionContext, _caseId);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        _fileTransferClientMock.Verify(c => c.GetFileTransferStatusAsync(transferId.ToString(), _correlationId, null), Times.Exactly(2));
        _caseMetadataServiceMock.Verify(s => s.CreateNetAppConnectionAsync(It.IsAny<CreateNetAppConnectionDto>()), Times.Once);
    }

    [Fact]
    public async Task Run_WhenFileTransferReturns202_PollsForCompletion()
    {
        // Arrange
        var dto = _fixture.Create<ProvisionNetAppFoldersDto>();
        var cmsArg = _fixture.Create<DdeiCaseIdArgDto>();
        var caseResponse = _fixture.Create<CaseDto>();
        var transferId = Guid.NewGuid();

        SetupValidRequest(dto, cmsArg);

        _ddeiClientMock.Setup(c => c.GetCaseAsync(cmsArg)).ReturnsAsync(caseResponse);
        _caseMetadataServiceMock.Setup(s => s.GetCaseMetadataForCaseIdAsync(_caseId)).ReturnsAsync((CaseMetadata?)null);
        _caseNamingServiceMock.Setup(s => s.GenerateCaseName(caseResponse)).ReturnsAsync(_caseNameDto);

        _fileTransferClientMock
            .Setup(c => c.ProvisionNetAppFoldersAsync(It.IsAny<ProvisionNetAppFoldersRequest>(), _correlationId))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent(JsonSerializer.Serialize(new { id = transferId }), Encoding.UTF8, "application/json")
            });

        _fileTransferClientMock
            .Setup(c => c.GetFileTransferStatusAsync(transferId.ToString(), _correlationId, null))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new { status = "Completed" }), Encoding.UTF8, "application/json")
            });

        var request = HttpRequestStubHelper.CreateHttpRequestFor(dto);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _cmsAuthValues, _username, _bearerToken);

        // Act
        var result = await _function.Run(request, functionContext, _caseId);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        _fileTransferClientMock.Verify(c => c.GetFileTransferStatusAsync(transferId.ToString(), _correlationId, null), Times.Once);
        _caseMetadataServiceMock.Verify(s => s.CreateNetAppConnectionAsync(It.IsAny<CreateNetAppConnectionDto>()), Times.Once);
    }

    [Fact]
    public async Task Run_WhenFileTransferReturnsConflict_ReturnsConflict()
    {
        // Arrange
        var dto = _fixture.Create<ProvisionNetAppFoldersDto>();
        var cmsArg = _fixture.Create<DdeiCaseIdArgDto>();
        var caseResponse = _fixture.Create<CaseDto>();
        var conflictMessage = "Conflict: resource already exists.";

        SetupValidRequest(dto, cmsArg);

        _ddeiClientMock.Setup(c => c.GetCaseAsync(cmsArg)).ReturnsAsync(caseResponse);
        _caseMetadataServiceMock.Setup(s => s.GetCaseMetadataForCaseIdAsync(_caseId)).ReturnsAsync((CaseMetadata?)null);
        _caseNamingServiceMock.Setup(s => s.GenerateCaseName(caseResponse)).ReturnsAsync(_caseNameDto);

        _fileTransferClientMock
            .Setup(c => c.ProvisionNetAppFoldersAsync(It.IsAny<ProvisionNetAppFoldersRequest>(), _correlationId))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Conflict)
            {
                Content = new StringContent(conflictMessage, Encoding.UTF8, "text/plain")
            });

        var request = HttpRequestStubHelper.CreateHttpRequestFor(dto);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _cmsAuthValues, _username, _bearerToken);

        // Act
        var result = await _function.Run(request, functionContext, _caseId);

        // Assert
        var conflict = Assert.IsType<ConflictObjectResult>(result);
        Assert.Equal(conflictMessage, conflict.Value?.ToString());
        _caseMetadataServiceMock.Verify(s => s.CreateNetAppConnectionAsync(It.IsAny<CreateNetAppConnectionDto>()), Times.Never);
    }

    [Fact]
    public async Task Run_WhenFileTransferReturnsServiceUnavailable_Returns503()
    {
        // Arrange
        var dto = _fixture.Create<ProvisionNetAppFoldersDto>();
        var cmsArg = _fixture.Create<DdeiCaseIdArgDto>();
        var caseResponse = _fixture.Create<CaseDto>();

        SetupValidRequest(dto, cmsArg);

        _ddeiClientMock.Setup(c => c.GetCaseAsync(cmsArg)).ReturnsAsync(caseResponse);
        _caseMetadataServiceMock.Setup(s => s.GetCaseMetadataForCaseIdAsync(_caseId)).ReturnsAsync((CaseMetadata?)null);
        _caseNamingServiceMock.Setup(s => s.GenerateCaseName(caseResponse)).ReturnsAsync(_caseNameDto);

        _fileTransferClientMock
            .Setup(c => c.ProvisionNetAppFoldersAsync(It.IsAny<ProvisionNetAppFoldersRequest>(), _correlationId))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
            {
                Content = new StringContent("Unable to verify destination folder state. Provisioning aborted.", Encoding.UTF8, "text/plain")
            });

        var request = HttpRequestStubHelper.CreateHttpRequestFor(dto);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _cmsAuthValues, _username, _bearerToken);

        // Act
        var result = await _function.Run(request, functionContext, _caseId);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(503, objectResult.StatusCode);
        _caseMetadataServiceMock.Verify(s => s.CreateNetAppConnectionAsync(It.IsAny<CreateNetAppConnectionDto>()), Times.Never);
    }

    [Fact]
    public async Task Run_WhenFileTransferReturns202AndOrchestrationFails_Returns500()
    {
        // Arrange
        var dto = _fixture.Create<ProvisionNetAppFoldersDto>();
        var cmsArg = _fixture.Create<DdeiCaseIdArgDto>();
        var caseResponse = _fixture.Create<CaseDto>();
        var transferId = Guid.NewGuid();

        SetupValidRequest(dto, cmsArg);

        _ddeiClientMock.Setup(c => c.GetCaseAsync(cmsArg)).ReturnsAsync(caseResponse);
        _caseMetadataServiceMock.Setup(s => s.GetCaseMetadataForCaseIdAsync(_caseId)).ReturnsAsync((CaseMetadata?)null);
        _caseNamingServiceMock.Setup(s => s.GenerateCaseName(caseResponse)).ReturnsAsync(_caseNameDto);

        _fileTransferClientMock
            .Setup(c => c.ProvisionNetAppFoldersAsync(It.IsAny<ProvisionNetAppFoldersRequest>(), _correlationId))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent(JsonSerializer.Serialize(new { id = transferId }), Encoding.UTF8, "application/json")
            });

        _fileTransferClientMock
            .Setup(c => c.GetFileTransferStatusAsync(transferId.ToString(), _correlationId, null))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new { status = "Failed" }), Encoding.UTF8, "application/json")
            });

        var request = HttpRequestStubHelper.CreateHttpRequestFor(dto);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _cmsAuthValues, _username, _bearerToken);

        // Act
        var result = await _function.Run(request, functionContext, _caseId);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        _caseMetadataServiceMock.Verify(s => s.CreateNetAppConnectionAsync(It.IsAny<CreateNetAppConnectionDto>()), Times.Never);
    }

    private void SetupValidRequest(ProvisionNetAppFoldersDto dto, DdeiCaseIdArgDto cmsArg)
    {
        _requestValidatorMock
            .Setup(v => v.GetJsonBody<ProvisionNetAppFoldersDto, ProvisionNetAppFoldersRequestValidator>(It.IsAny<HttpRequest>()))
            .ReturnsAsync(new ValidatableRequest<ProvisionNetAppFoldersDto>
            {
                IsValid = true,
                Value = dto
            });

        _ddeiArgFactoryMock
            .Setup(f => f.CreateCaseArg(_cmsAuthValues, _correlationId, _caseId))
            .Returns(cmsArg);
    }

    private void SetupSuccessfulTransfer(CaseDto caseResponse, string terminalStatus)
    {
        var transferId = Guid.NewGuid();

        _ddeiClientMock.Setup(c => c.GetCaseAsync(It.IsAny<DdeiCaseIdArgDto>())).ReturnsAsync(caseResponse);
        _caseMetadataServiceMock.Setup(s => s.GetCaseMetadataForCaseIdAsync(_caseId)).ReturnsAsync((CaseMetadata?)null);
        _caseNamingServiceMock.Setup(s => s.GenerateCaseName(caseResponse)).ReturnsAsync(_caseNameDto);

        _fileTransferClientMock
            .Setup(c => c.ProvisionNetAppFoldersAsync(It.IsAny<ProvisionNetAppFoldersRequest>(), _correlationId))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { id = transferId }),
                    Encoding.UTF8, "application/json")
            });

        _fileTransferClientMock
            .Setup(c => c.GetFileTransferStatusAsync(transferId.ToString(), _correlationId, null))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new { status = terminalStatus }),
                    Encoding.UTF8, "application/json")
            });
    }
}
