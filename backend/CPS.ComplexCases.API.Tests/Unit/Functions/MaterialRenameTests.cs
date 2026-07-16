using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AutoFixture;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.API.Domain.Models;
using CPS.ComplexCases.API.Domain.Response;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using CPS.ComplexCases.API.Validators.Requests;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models;
using CPS.ComplexCases.Common.Models.Configuration;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Enums;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Exceptions;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Models.Args;
using CPS.ComplexCases.NetApp.Models.Requests;
using Moq;
using CPS.ComplexCases.NetApp.Client;

namespace CPS.ComplexCases.API.Tests.Unit.Functions;

public class MaterialBatchRenameTests
{
    private readonly Mock<ILogger<MaterialBatchRename>> _loggerMock;
    private readonly Mock<IActivityLogService> _activityLogServiceMock;
    private readonly Mock<IRequestValidator> _requestValidatorMock;
    private readonly Mock<IInitializationHandler> _initializationHandlerMock;
    private readonly Mock<ISecurityGroupMetadataService> _securityGroupMetadataServiceMock;
    private readonly Mock<ICaseMetadataService> _caseMetadataServiceMock;
    private readonly Mock<IOntapArgFactory> _ontapArgFactoryMock;
    private readonly Mock<IOntapHttpClient> _ontapHttpClientMock;

    private readonly Fixture _fixture;
    private readonly Guid _testCorrelationId;
    private readonly string _testUsername;
    private readonly string _testCmsAuthValues;
    private readonly string _testBearerToken;

    public MaterialBatchRenameTests()
    {
        _loggerMock = new Mock<ILogger<MaterialBatchRename>>();
        _activityLogServiceMock = new Mock<IActivityLogService>();
        _requestValidatorMock = new Mock<IRequestValidator>();
        _initializationHandlerMock = new Mock<IInitializationHandler>();
        _securityGroupMetadataServiceMock = new Mock<ISecurityGroupMetadataService>();
        _caseMetadataServiceMock = new Mock<ICaseMetadataService>();
        _ontapArgFactoryMock = new Mock<IOntapArgFactory>();
        _ontapHttpClientMock = new Mock<IOntapHttpClient>();

        _fixture = new Fixture();
        _testCorrelationId = _fixture.Create<Guid>();
        _testUsername = _fixture.Create<string>();
        _testCmsAuthValues = _fixture.Create<string>();
        _testBearerToken = _fixture.Create<string>();
    }

    [Fact]
    public async Task Run_WhenFeatureFlagIsDisabled_ReturnsNotFound()
    {
        // Arrange
        var function = CreateFunction(materialRenameEnabled: false);
        var request = HttpRequestStubHelper.CreateHttpRequest();
        var context = CreateFunctionContext();

        // Act
        var result = await function.Run(request, context);

        // Assert
        Assert.IsType<NotFoundResult>(result);
        _requestValidatorMock.Verify(
            v => v.GetJsonBody<MaterialBatchRenameRequestDto, MaterialRenameRequestValidator>(It.IsAny<HttpRequest>()),
            Times.Never);
        _securityGroupMetadataServiceMock.Verify(
            s => s.GetUserSecurityGroupsAsync(It.IsAny<string>()),
            Times.Never);
        _ontapHttpClientMock.Verify(
            c => c.RenameMaterialAsync(It.IsAny<MaterialRenameArg>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_WhenRequestIsInvalid_ReturnsBadRequestWithValidationErrors()
    {
        // Arrange
        var validationErrors = new List<string> { "Operations list is required.", "CaseId is required." };
        var requestDto = CreateValidRequestDto();
        SetupRequestValidator(requestDto, isValid: false, validationErrors);

        var function = CreateFunction(materialRenameEnabled: true);
        var request = HttpRequestStubHelper.CreateHttpRequestFor(requestDto);
        var context = CreateFunctionContext();

        // Act
        var result = await function.Run(request, context);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(validationErrors, badRequest.Value);

        _initializationHandlerMock.Verify(
            i => i.Initialize(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<int?>()),
            Times.Never);
        _securityGroupMetadataServiceMock.Verify(
            s => s.GetUserSecurityGroupsAsync(It.IsAny<string>()),
            Times.Never);
        _ontapHttpClientMock.Verify(
            c => c.RenameMaterialAsync(It.IsAny<MaterialRenameArg>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_WhenCaseMetadataIsMissing_ReturnsBadRequest()
    {
        // Arrange
        var requestDto = CreateValidRequestDto();
        SetupRequestValidator(requestDto, isValid: true);
        _caseMetadataServiceMock
            .Setup(s => s.GetCaseMetadataForCaseIdAsync(requestDto.CaseId))
            .ReturnsAsync(null as CaseMetadata);

        var function = CreateFunction(materialRenameEnabled: true);
        var request = HttpRequestStubHelper.CreateHttpRequestFor(requestDto);
        var context = CreateFunctionContext();

        // Act
        var result = await function.Run(request, context);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var errors = Assert.IsType<string[]>(badRequest.Value);
        Assert.Contains("Case metadata or NetApp folder path is missing.", errors);
    }

    [Fact]
    public async Task Run_WhenValidRequest_CallsInitializationHandler()
    {
        // Arrange
        var requestDto = CreateValidRequestDto();
        SetupRequestValidator(requestDto, isValid: true);
        SetupCaseMetadata(requestDto.CaseId);
        SetupSecurityGroups();
        SetupOntapClientForSuccessfulRename();

        var function = CreateFunction(materialRenameEnabled: true);
        var request = HttpRequestStubHelper.CreateHttpRequestFor(requestDto);
        var context = CreateFunctionContext();

        // Act
        await function.Run(request, context);

        // Assert
        _initializationHandlerMock.Verify(
            i => i.Initialize(_testUsername, _testCorrelationId, requestDto.CaseId),
            Times.Once);
    }

    [Fact]
    public async Task Run_WhenOperationPathIsOutsideCaseFolder_IncludesFailureInResults()
    {
        // Arrange
        var requestDto = new MaterialBatchRenameRequestDto
        {
            CaseId = 42,
            Operations = new List<RenameNetAppMaterialBatchOperationDto>
            {
                new()
                {
                    Type = NetAppOperationType.Material,
                    CurrentPath = "outside/path",  // Outside case prefix
                    NewPath = "CASE-PREFIX/new-path"
                }
            }
        };
        SetupRequestValidator(requestDto, isValid: true);
        SetupCaseMetadata(requestDto.CaseId, "CASE-PREFIX");
        SetupSecurityGroups();

        var function = CreateFunction(materialRenameEnabled: true);
        var request = HttpRequestStubHelper.CreateHttpRequestFor(requestDto);
        var context = CreateFunctionContext();

        // Act
        var result = await function.Run(request, context);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<MaterialRenameBatchResponse>(okResult.Value);
        Assert.Equal(1, response.TotalRequested);
        Assert.Equal(1, response.Failed);
        _ontapHttpClientMock.Verify(
            c => c.RenameMaterialAsync(It.IsAny<MaterialRenameArg>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_WhenMaterialNotFound_IncludesNotFoundInResults()
    {
        // Arrange
        var requestDto = CreateValidRequestDto();
        SetupRequestValidator(requestDto, isValid: true);
        SetupCaseMetadata(requestDto.CaseId);
        SetupSecurityGroups();
        SetupOntapClientForNotFoundResult();

        var function = CreateFunction(materialRenameEnabled: true);
        var request = HttpRequestStubHelper.CreateHttpRequestFor(requestDto);
        var context = CreateFunctionContext();

        // Act
        var result = await function.Run(request, context);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<MaterialRenameBatchResponse>(okResult.Value);
        Assert.Equal(1, response.TotalRequested);
        Assert.Equal(1, response.NotFound);
        Assert.Equal(0, response.Failed);
        _ontapHttpClientMock.Verify(
            c => c.RenameMaterialAsync(It.IsAny<MaterialRenameArg>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_WhenOntapUnauthorizedExceptionOccurs_PropagatesUnauthorized()
    {
        // Arrange
        var requestDto = CreateValidRequestDto();
        SetupRequestValidator(requestDto, isValid: true);
        SetupCaseMetadata(requestDto.CaseId);
        SetupSecurityGroups();
        SetupOntapClientForException(new OntapUnauthorizedException("Unauthorized access to ONTAP."));

        var function = CreateFunction(materialRenameEnabled: true);
        var request = HttpRequestStubHelper.CreateHttpRequestFor(requestDto);
        var context = CreateFunctionContext();

        // Act
        var exception = await Assert.ThrowsAsync<OntapUnauthorizedException>(() => function.Run(request, context));

        // Assert
        Assert.Equal("Unauthorized access to ONTAP.", exception.Message);
        _activityLogServiceMock.Verify(
            s => s.CreateActivityLogAsync(
                It.IsAny<ActivityLog.Enums.ActionType>(),
                It.IsAny<ActivityLog.Enums.ResourceType>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<System.Text.Json.JsonDocument?>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_WhenAuthFailsMidBatch_WritesActivityLogForCompletedRenamesThenRethrows()
    {
        var requestDto = new MaterialBatchRenameRequestDto
        {
            CaseId = 42,
            Operations =
            [
                new()
                {
                    Type = NetAppOperationType.Material,
                    CurrentPath = "CASE-PREFIX/file1.txt",
                    NewPath = "CASE-PREFIX/renamed1.txt"
                },
                new()
                {
                    Type = NetAppOperationType.Material,
                    CurrentPath = "CASE-PREFIX/file2.txt",
                    NewPath = "CASE-PREFIX/renamed2.txt"
                },
                new()
                {
                    Type = NetAppOperationType.Material,
                    CurrentPath = "CASE-PREFIX/file3.txt",
                    NewPath = "CASE-PREFIX/renamed3.txt"
                }
            ]
        };
        SetupRequestValidator(requestDto, isValid: true);
        SetupCaseMetadata(requestDto.CaseId);
        SetupSecurityGroups();

        _ontapArgFactoryMock
            .Setup(f => f.CreateMaterialRenameArg(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns((string token, Guid uuid, string currentPath, string newPath) =>
                new MaterialRenameArg
                {
                    BearerToken = token,
                    OntapVolumeUuid = uuid,
                    CurrentFilePath = currentPath,
                    NewFilePath = newPath
                });

        var success = new MaterialRenameResult(Success: true, WasFound: true, KeysRenamed: 1, ErrorMessage: null, ErrorStatusCode: null);
        _ontapHttpClientMock
            .SetupSequence(c => c.RenameMaterialAsync(It.IsAny<MaterialRenameArg>()))
            .ReturnsAsync(success)
            .ReturnsAsync(success)
            .ThrowsAsync(new OntapUnauthorizedException("Unauthorized access to ONTAP."));

        var function = CreateFunction(materialRenameEnabled: true);
        var request = HttpRequestStubHelper.CreateHttpRequestFor(requestDto);
        var context = CreateFunctionContext();

        var exception = await Assert.ThrowsAsync<OntapUnauthorizedException>(() => function.Run(request, context));

        Assert.Equal("Unauthorized access to ONTAP.", exception.Message);
        _activityLogServiceMock.Verify(
            s => s.CreateActivityLogAsync(
                ActivityLog.Enums.ActionType.MaterialRenamed,
                ActivityLog.Enums.ResourceType.Material,
                requestDto.CaseId,
                requestDto.CaseId.ToString(),
                null,
                _testUsername,
                It.IsAny<System.Text.Json.JsonDocument?>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_WhenOntapForbiddenExceptionOccurs_PropagatesForbidden()
    {
        // Arrange
        var requestDto = CreateValidRequestDto();
        SetupRequestValidator(requestDto, isValid: true);
        SetupCaseMetadata(requestDto.CaseId);
        SetupSecurityGroups();
        SetupOntapClientForException(new OntapClientException(System.Net.HttpStatusCode.Forbidden, new HttpRequestException("Forbidden access to ONTAP.")));

        var function = CreateFunction(materialRenameEnabled: true);
        var request = HttpRequestStubHelper.CreateHttpRequestFor(requestDto);
        var context = CreateFunctionContext();

        // Act
        var exception = await Assert.ThrowsAsync<OntapClientException>(() => function.Run(request, context));

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Forbidden, exception.StatusCode);
    }

    private MaterialBatchRename CreateFunction(bool materialRenameEnabled)
    {
        var featureFlags = Options.Create(new FeatureFlagConfig
        {
            MaterialRename = materialRenameEnabled
        });

        return new MaterialBatchRename(
            _loggerMock.Object,
            _activityLogServiceMock.Object,
            _requestValidatorMock.Object,
            _initializationHandlerMock.Object,
            _securityGroupMetadataServiceMock.Object,
            _caseMetadataServiceMock.Object,
            _ontapArgFactoryMock.Object,
            _ontapHttpClientMock.Object,
            featureFlags);
    }

    private void SetupRequestValidator(MaterialBatchRenameRequestDto dto, bool isValid, List<string>? validationErrors = null)
    {
        _requestValidatorMock
            .Setup(v => v.GetJsonBody<MaterialBatchRenameRequestDto, MaterialRenameRequestValidator>(It.IsAny<HttpRequest>()))
            .ReturnsAsync(new ValidatableRequest<MaterialBatchRenameRequestDto>
            {
                Value = dto,
                IsValid = isValid,
                ValidationErrors = validationErrors ?? []
            });
    }

    private void SetupCaseMetadata(int caseId, string netappPath = "CASE-PREFIX")
    {
        _caseMetadataServiceMock
            .Setup(s => s.GetCaseMetadataForCaseIdAsync(caseId))
            .ReturnsAsync(new CaseMetadata
            {
                CaseId = caseId,
                NetappFolderPath = netappPath
            });
    }

    private void SetupSecurityGroups()
    {
        var volumeUuid = _fixture.Create<Guid>();
        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(_testBearerToken))
            .ReturnsAsync(new List<SecurityGroup>
            {
                new()
                {
                    Id = _fixture.Create<Guid>(),
                    DisplayName = "TestSG",
                    BucketName = "test-bucket",
                    VolumeUuid = volumeUuid
                }
            });
    }

    private void SetupOntapClientForSuccessfulRename()
    {
        _ontapArgFactoryMock
            .Setup(f => f.CreateMaterialRenameArg(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns((string token, Guid uuid, string currentPath, string newPath) =>
                new MaterialRenameArg
                {
                    BearerToken = token,
                    OntapVolumeUuid = uuid,
                    CurrentFilePath = currentPath,
                    NewFilePath = newPath
                });

        _ontapHttpClientMock
            .Setup(c => c.RenameMaterialAsync(It.IsAny<MaterialRenameArg>()))
            .ReturnsAsync(new MaterialRenameResult(Success: true, WasFound: true, KeysRenamed: 1, ErrorMessage: null, ErrorStatusCode: null));
    }

    private void SetupOntapClientForNotFoundResult()
    {
        _ontapArgFactoryMock
            .Setup(f => f.CreateMaterialRenameArg(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns((string token, Guid uuid, string currentPath, string newPath) =>
                new MaterialRenameArg
                {
                    BearerToken = token,
                    OntapVolumeUuid = uuid,
                    CurrentFilePath = currentPath,
                    NewFilePath = newPath
                });

        _ontapHttpClientMock
            .Setup(c => c.RenameMaterialAsync(It.IsAny<MaterialRenameArg>()))
            .ReturnsAsync(new MaterialRenameResult(Success: false, WasFound: false, KeysRenamed: 0, ErrorMessage: "Not found", ErrorStatusCode: 404));
    }

    private void SetupOntapClientForException(Exception exception)
    {
        _ontapArgFactoryMock
            .Setup(f => f.CreateMaterialRenameArg(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns((string token, Guid uuid, string currentPath, string newPath) =>
                new MaterialRenameArg
                {
                    BearerToken = token,
                    OntapVolumeUuid = uuid,
                    CurrentFilePath = currentPath,
                    NewFilePath = newPath
                });

        _ontapHttpClientMock
            .Setup(c => c.RenameMaterialAsync(It.IsAny<MaterialRenameArg>()))
            .ThrowsAsync(exception);
    }

    private FunctionContext CreateFunctionContext() =>
        FunctionContextStubHelper.CreateFunctionContextStub(
            _testCorrelationId,
            _testCmsAuthValues,
            _testUsername,
            _testBearerToken);

    private static MaterialBatchRenameRequestDto CreateValidRequestDto() =>
        new()
        {
            CaseId = 42,
            Operations = new List<RenameNetAppMaterialBatchOperationDto>
            {
                new()
                {
                    Type = NetAppOperationType.Material,
                    CurrentPath = "CASE-PREFIX/old-path",
                    NewPath = "CASE-PREFIX/new-path"
                }
            }
        };
}
