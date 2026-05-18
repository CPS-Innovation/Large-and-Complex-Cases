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

        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(It.IsAny<string>()))
            .ReturnsAsync([
                new SecurityGroup
                {
                    Id = _fixture.Create<Guid>(),
                    BucketName = _bucketName,
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

        _ddeiClientMock
            .Setup(c => c.GetCaseAsync(cmsArg))
            .ReturnsAsync(caseResponse);

        _caseMetadataServiceMock
            .Setup(s => s.GetCaseMetadataForCaseIdAsync(_caseId))
            .ReturnsAsync((CaseMetadata?)null);

        _caseNamingServiceMock
            .Setup(s => s.GenerateCaseName(caseResponse))
            .ReturnsAsync(_caseName);

        _fileTransferClientMock
            .Setup(c => c.ProvisionNetAppFoldersAsync(It.IsAny<ProvisionNetAppFoldersRequest>(), _correlationId))
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

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

        _ddeiClientMock
            .Setup(c => c.GetCaseAsync(cmsArg))
            .ReturnsAsync(caseResponse);

        _caseMetadataServiceMock
            .Setup(s => s.GetCaseMetadataForCaseIdAsync(_caseId))
            .ReturnsAsync((CaseMetadata?)null);

        _caseNamingServiceMock
            .Setup(s => s.GenerateCaseName(caseResponse))
            .ReturnsAsync(_caseName);

        _fileTransferClientMock
            .Setup(c => c.ProvisionNetAppFoldersAsync(It.IsAny<ProvisionNetAppFoldersRequest>(), _correlationId))
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

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

        _ddeiClientMock
            .Setup(c => c.GetCaseAsync(cmsArg))
            .ReturnsAsync(caseResponse);

        _caseMetadataServiceMock
            .Setup(s => s.GetCaseMetadataForCaseIdAsync(_caseId))
            .ReturnsAsync((CaseMetadata?)null);

        _caseNamingServiceMock
            .Setup(s => s.GenerateCaseName(caseResponse))
            .ReturnsAsync(_caseName);

        _fileTransferClientMock
            .Setup(c => c.ProvisionNetAppFoldersAsync(It.IsAny<ProvisionNetAppFoldersRequest>(), _correlationId))
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

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
    public async Task Run_WhenRequestIsValid_ReturnsOkWithFolderPath()
    {
        // Arrange
        var dto = _fixture.Create<ProvisionNetAppFoldersDto>();
        var cmsArg = _fixture.Create<DdeiCaseIdArgDto>();
        var caseResponse = _fixture.Create<CaseDto>();
        var expectedFolderPath = $"{_caseName}/";

        SetupValidRequest(dto, cmsArg);

        _ddeiClientMock
            .Setup(c => c.GetCaseAsync(cmsArg))
            .ReturnsAsync(caseResponse);

        _caseMetadataServiceMock
            .Setup(s => s.GetCaseMetadataForCaseIdAsync(_caseId))
            .ReturnsAsync((CaseMetadata?)null);

        _caseNamingServiceMock
            .Setup(s => s.GenerateCaseName(caseResponse))
            .ReturnsAsync(_caseName);

        _fileTransferClientMock
            .Setup(c => c.ProvisionNetAppFoldersAsync(It.IsAny<ProvisionNetAppFoldersRequest>(), _correlationId))
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

        var request = HttpRequestStubHelper.CreateHttpRequestFor(dto);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _cmsAuthValues, _username, _bearerToken);

        // Act
        var result = await _function.Run(request, functionContext, _caseId);

        // Assert
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

        _ddeiClientMock
            .Setup(c => c.GetCaseAsync(cmsArg))
            .ReturnsAsync(caseResponse);

        _caseMetadataServiceMock
            .Setup(s => s.GetCaseMetadataForCaseIdAsync(_caseId))
            .ReturnsAsync((CaseMetadata?)null);

        _caseNamingServiceMock
            .Setup(s => s.GenerateCaseName(caseResponse))
            .ReturnsAsync(_caseName);

        _fileTransferClientMock
            .Setup(c => c.ProvisionNetAppFoldersAsync(It.IsAny<ProvisionNetAppFoldersRequest>(), _correlationId))
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

        var request = HttpRequestStubHelper.CreateHttpRequestFor(dto);
        var functionContext = FunctionContextStubHelper.CreateFunctionContextStub(_correlationId, _cmsAuthValues, _username, _bearerToken);

        // Act
        var result = await _function.Run(request, functionContext, _caseId);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        _fileTransferClientMock.Verify(c => c.ProvisionNetAppFoldersAsync(It.IsAny<ProvisionNetAppFoldersRequest>(), _correlationId), Times.Once);
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
}
