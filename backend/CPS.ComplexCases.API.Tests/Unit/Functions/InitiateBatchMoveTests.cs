using System.Net;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
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
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.Data.Models.Requests;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Constants;
using CPS.ComplexCases.NetApp.Exceptions;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Models.Args;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions;

public class InitiateBatchMoveTests
{
    private readonly Mock<ILogger<InitiateBatchMove>> _loggerMock;
    private readonly Mock<IRequestValidator> _requestValidatorMock;
    private readonly Mock<ISecurityGroupMetadataService> _securityGroupMetadataServiceMock;
    private readonly Mock<ICaseMetadataService> _caseMetadataServiceMock;
    private readonly Mock<IInitializationHandler> _initializationHandlerMock;
    private readonly Mock<IActivityLogService> _activityLogServiceMock;
    private readonly Mock<ICaseActiveManageMaterialsService> _caseActiveManageMaterialsServiceMock;
    private readonly Mock<IOntapArgFactory> _ontapArgFactoryMock;
    private readonly Mock<IOntapHttpClient> _ontapHttpClientMock;
    private readonly InitiateBatchMove _function;
    private readonly Fixture _fixture;

    private readonly string _testBearerToken;
    private readonly Guid _testCorrelationId;
    private readonly string _testUsername;
    private readonly string _testCmsAuthValues;

    private const string TestNetAppFolder = "CaseRoot";

    public InitiateBatchMoveTests()
    {
        _loggerMock = new Mock<ILogger<InitiateBatchMove>>();
        _requestValidatorMock = new Mock<IRequestValidator>();
        _securityGroupMetadataServiceMock = new Mock<ISecurityGroupMetadataService>();
        _caseMetadataServiceMock = new Mock<ICaseMetadataService>();
        _initializationHandlerMock = new Mock<IInitializationHandler>();
        _activityLogServiceMock = new Mock<IActivityLogService>();
        _caseActiveManageMaterialsServiceMock = new Mock<ICaseActiveManageMaterialsService>();
        _ontapArgFactoryMock = new Mock<IOntapArgFactory>();
        _ontapHttpClientMock = new Mock<IOntapHttpClient>();

        _fixture = new Fixture();
        _testBearerToken = _fixture.Create<string>();
        _testCorrelationId = _fixture.Create<Guid>();
        _testUsername = _fixture.Create<string>();
        _testCmsAuthValues = _fixture.Create<string>();

        _caseMetadataServiceMock
            .Setup(s => s.GetCaseMetadataForCaseIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new CaseMetadata { CaseId = 1, NetappFolderPath = TestNetAppFolder });

        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(It.IsAny<string>()))
            .ReturnsAsync([new SecurityGroup { Id = _fixture.Create<Guid>(), BucketName = "test-bucket", VolumeUuid = _fixture.Create<Guid>(), DisplayName = "Test" }]);

        _caseActiveManageMaterialsServiceMock
            .Setup(s => s.CheckConflictAndInsertAsync(
                It.IsAny<CaseActiveManageMaterialsOperation>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(true);

        _caseActiveManageMaterialsServiceMock
            .Setup(s => s.DeleteOperationAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        _activityLogServiceMock
            .Setup(s => s.CreateActivityLogAsync(
                It.IsAny<ActivityLog.Enums.ActionType>(),
                It.IsAny<ActivityLog.Enums.ResourceType>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<System.Text.Json.JsonDocument?>()))
            .Returns(Task.CompletedTask);

        _function = CreateFunction();
    }

    [Fact]
    public async Task Run_WithInvalidRequest_ReturnsBadRequest()
    {
        SetupInvalidRequest();
        var req = HttpRequestStubHelper.CreateHttpRequest(_testCorrelationId);

        var result = await _function.Run(req, CreateFunctionContext());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Run_WhenCaseMetadataIsNull_ReturnsBadRequest()
    {
        SetupValidRequest(ValidDto());
        _caseMetadataServiceMock
            .Setup(s => s.GetCaseMetadataForCaseIdAsync(It.IsAny<int>()))
            .ReturnsAsync((CaseMetadata?)null);

        var req = HttpRequestStubHelper.CreateHttpRequest(_testCorrelationId);

        var result = await _function.Run(req, CreateFunctionContext());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Run_WhenSourcePathOutsideCaseFolder_ReturnsBadRequest()
    {
        var dto = new MoveNetAppBatchDto
        {
            CaseId = 1,
            DestinationPrefix = $"{TestNetAppFolder}/Folder-B/",
            Operations = [new() { Type = NetAppBatchOperationType.Material, SourcePath = "OtherCase/file.txt" }]
        };
        SetupValidRequest(dto);
        var req = HttpRequestStubHelper.CreateHttpRequest(_testCorrelationId);

        var result = await _function.Run(req, CreateFunctionContext());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Run_WhenDestinationPrefixOutsideCaseFolder_ReturnsBadRequest()
    {
        var dto = new MoveNetAppBatchDto
        {
            CaseId = 1,
            DestinationPrefix = "OtherCase/Evidence/",
            Operations = [new() { Type = NetAppBatchOperationType.Material, SourcePath = $"{TestNetAppFolder}/file.txt" }]
        };
        SetupValidRequest(dto);
        var req = HttpRequestStubHelper.CreateHttpRequest(_testCorrelationId);

        var result = await _function.Run(req, CreateFunctionContext());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Run_WhenDestinationPrefixOutsideCaseFolder_DoesNotCallOntap()
    {
        _caseMetadataServiceMock
            .Setup(s => s.GetCaseMetadataForCaseIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new CaseMetadata { CaseId = 1, NetappFolderPath = "Cases/123" });

        var dto = new MoveNetAppBatchDto
        {
            CaseId = 1,
            DestinationPrefix = "Cases/999/",
            Operations = [new() { Type = NetAppBatchOperationType.Material, SourcePath = "Cases/123/file.pdf" }]
        };
        SetupValidRequest(dto);
        var req = HttpRequestStubHelper.CreateHttpRequest(_testCorrelationId);

        var result = await _function.Run(req, CreateFunctionContext());

        Assert.IsType<BadRequestObjectResult>(result);
        _ontapHttpClientMock.Verify(
            c => c.RenameMaterialAsync(It.IsAny<MaterialRenameArg>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_WhenConflictLockNotAcquired_ReturnsConflict()
    {
        SetupValidRequest(ValidDto());
        _caseActiveManageMaterialsServiceMock
            .Setup(s => s.CheckConflictAndInsertAsync(
                It.IsAny<CaseActiveManageMaterialsOperation>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(false);

        var req = HttpRequestStubHelper.CreateHttpRequest(_testCorrelationId);

        var result = await _function.Run(req, CreateFunctionContext());

        Assert.IsType<ConflictObjectResult>(result);
        _ontapHttpClientMock.Verify(
            c => c.RenameMaterialAsync(It.IsAny<MaterialRenameArg>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_WithValidRequest_ReturnsOkWithMovedStatus()
    {
        SetupValidRequest(ValidDto());
        SetupOntapClientForSuccessfulMove();
        var req = HttpRequestStubHelper.CreateHttpRequest(_testCorrelationId);

        var result = await _function.Run(req, CreateFunctionContext());

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<MoveNetAppBatchResponse>(okResult.Value);
        Assert.Equal("Completed", response.Status);
        Assert.Equal(1, response.TotalRequested);
        Assert.Equal(1, response.Succeeded);
        Assert.Equal(OperationResultStatus.Moved, response.Results[0].Status);
        _ontapHttpClientMock.Verify(c => c.RenameMaterialAsync(It.IsAny<MaterialRenameArg>()), Times.Once);
    }

    [Fact]
    public async Task Run_WithValidRequest_ReleasesManageMaterialsLock()
    {
        SetupValidRequest(ValidDto());
        SetupOntapClientForSuccessfulMove();
        var req = HttpRequestStubHelper.CreateHttpRequest(_testCorrelationId);

        await _function.Run(req, CreateFunctionContext());

        _caseActiveManageMaterialsServiceMock.Verify(
            s => s.DeleteOperationAsync(It.IsAny<Guid>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_WhenOntapThrows_ReleasesManageMaterialsLock()
    {
        SetupValidRequest(ValidDto());
        SetupOntapClientForException(new InvalidOperationException("Unexpected failure"));
        var req = HttpRequestStubHelper.CreateHttpRequest(_testCorrelationId);

        var result = await _function.Run(req, CreateFunctionContext());

        Assert.IsType<OkObjectResult>(result);
        _caseActiveManageMaterialsServiceMock.Verify(
            s => s.DeleteOperationAsync(It.IsAny<Guid>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_WhenMaterialNotFound_IncludesNotFoundInResults()
    {
        SetupValidRequest(ValidDto());
        SetupOntapClientForNotFoundResult();
        var req = HttpRequestStubHelper.CreateHttpRequest(_testCorrelationId);

        var result = await _function.Run(req, CreateFunctionContext());

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<MoveNetAppBatchResponse>(okResult.Value);
        Assert.Equal("NoOp", response.Status);
        Assert.Equal(1, response.NotFound);
        Assert.Equal(OperationResultStatus.NotFound, response.Results[0].Status);
    }

    [Fact]
    public async Task Run_WhenOntapReturnsConflict_IncludesConflictInResults()
    {
        SetupValidRequest(ValidDto());
        SetupOntapClientForConflictResult();
        var req = HttpRequestStubHelper.CreateHttpRequest(_testCorrelationId);

        var result = await _function.Run(req, CreateFunctionContext());

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<MoveNetAppBatchResponse>(okResult.Value);
        Assert.Equal("Failed", response.Status);
        Assert.Equal(1, response.Failed);
        Assert.Equal(OperationResultStatus.Conflict, response.Results[0].Status);
    }

    [Fact]
    public async Task Run_WhenFolderMoveIntoOwnSubtree_ReturnsFailedWithoutCallingOntap()
    {
        var dto = new MoveNetAppBatchDto
        {
            CaseId = 1,
            DestinationPrefix = $"{TestNetAppFolder}/Folder-A/",
            Operations = [new() { Type = NetAppBatchOperationType.Folder, SourcePath = $"{TestNetAppFolder}/Folder-A/" }]
        };
        SetupValidRequest(dto);
        var req = HttpRequestStubHelper.CreateHttpRequest(_testCorrelationId);

        var result = await _function.Run(req, CreateFunctionContext());

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<MoveNetAppBatchResponse>(okResult.Value);
        Assert.Equal(1, response.Failed);
        Assert.Equal(OperationResultStatus.Failed, response.Results[0].Status);
        _ontapHttpClientMock.Verify(
            c => c.RenameMaterialAsync(It.IsAny<MaterialRenameArg>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_WhenOntapUnauthorizedExceptionOccurs_PropagatesUnauthorized()
    {
        SetupValidRequest(ValidDto());
        SetupOntapClientForException(new OntapUnauthorizedException("Unauthorized access to ONTAP."));
        var req = HttpRequestStubHelper.CreateHttpRequest(_testCorrelationId);

        var exception = await Assert.ThrowsAsync<OntapUnauthorizedException>(() => _function.Run(req, CreateFunctionContext()));

        Assert.Equal("Unauthorized access to ONTAP.", exception.Message);
    }

    [Fact]
    public async Task Run_WhenOntapForbiddenExceptionOccurs_PropagatesForbidden()
    {
        SetupValidRequest(ValidDto());
        SetupOntapClientForException(new OntapClientException(HttpStatusCode.Forbidden, new HttpRequestException("Forbidden access to ONTAP.")));
        var req = HttpRequestStubHelper.CreateHttpRequest(_testCorrelationId);

        var exception = await Assert.ThrowsAsync<OntapClientException>(() => _function.Run(req, CreateFunctionContext()));

        Assert.Equal(HttpStatusCode.Forbidden, exception.StatusCode);
    }

    private InitiateBatchMove CreateFunction() =>
        new(
            _loggerMock.Object,
            _requestValidatorMock.Object,
            _securityGroupMetadataServiceMock.Object,
            _caseMetadataServiceMock.Object,
            _initializationHandlerMock.Object,
            _activityLogServiceMock.Object,
            _caseActiveManageMaterialsServiceMock.Object,
            _ontapArgFactoryMock.Object,
            _ontapHttpClientMock.Object);

    private FunctionContext CreateFunctionContext() =>
        FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

    private void SetupValidRequest(MoveNetAppBatchDto dto)
    {
        _requestValidatorMock
            .Setup(v => v.GetJsonBody<MoveNetAppBatchDto, MoveNetAppBatchRequestValidator>(It.IsAny<HttpRequest>()))
            .ReturnsAsync(new ValidatableRequest<MoveNetAppBatchDto> { Value = dto, IsValid = true, ValidationErrors = [] });
    }

    private void SetupInvalidRequest()
    {
        _requestValidatorMock
            .Setup(v => v.GetJsonBody<MoveNetAppBatchDto, MoveNetAppBatchRequestValidator>(It.IsAny<HttpRequest>()))
            .ReturnsAsync(new ValidatableRequest<MoveNetAppBatchDto> { Value = null!, IsValid = false, ValidationErrors = ["CaseId must be a positive integer."] });
    }

    private static MoveNetAppBatchDto ValidDto(int caseId = 1) => new()
    {
        CaseId = caseId,
        DestinationPrefix = $"{TestNetAppFolder}/Folder-B/",
        Operations = [new() { Type = NetAppBatchOperationType.Material, SourcePath = $"{TestNetAppFolder}/file.txt" }]
    };

    private void SetupOntapClientForSuccessfulMove()
    {
        _ontapArgFactoryMock
            .Setup(f => f.CreateMaterialRenameArg(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns((string token, Guid uuid, string sourcePath, string destinationPath) =>
                new MaterialRenameArg
                {
                    BearerToken = token,
                    OntapVolumeUuid = uuid,
                    CurrentFilePath = sourcePath,
                    NewFilePath = destinationPath
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
            .Returns((string token, Guid uuid, string sourcePath, string destinationPath) =>
                new MaterialRenameArg
                {
                    BearerToken = token,
                    OntapVolumeUuid = uuid,
                    CurrentFilePath = sourcePath,
                    NewFilePath = destinationPath
                });

        _ontapHttpClientMock
            .Setup(c => c.RenameMaterialAsync(It.IsAny<MaterialRenameArg>()))
            .ReturnsAsync(new MaterialRenameResult(Success: false, WasFound: false, KeysRenamed: 0, ErrorMessage: "Not found", ErrorStatusCode: 404));
    }

    private void SetupOntapClientForConflictResult()
    {
        _ontapArgFactoryMock
            .Setup(f => f.CreateMaterialRenameArg(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns((string token, Guid uuid, string sourcePath, string destinationPath) =>
                new MaterialRenameArg
                {
                    BearerToken = token,
                    OntapVolumeUuid = uuid,
                    CurrentFilePath = sourcePath,
                    NewFilePath = destinationPath
                });

        _ontapHttpClientMock
            .Setup(c => c.RenameMaterialAsync(It.IsAny<MaterialRenameArg>()))
            .ReturnsAsync(new MaterialRenameResult(Success: false, WasFound: true, KeysRenamed: 0, ErrorMessage: "Conflict", ErrorStatusCode: (int)HttpStatusCode.Conflict));
    }

    private void SetupOntapClientForException(Exception exception)
    {
        _ontapArgFactoryMock
            .Setup(f => f.CreateMaterialRenameArg(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns((string token, Guid uuid, string sourcePath, string destinationPath) =>
                new MaterialRenameArg
                {
                    BearerToken = token,
                    OntapVolumeUuid = uuid,
                    CurrentFilePath = sourcePath,
                    NewFilePath = destinationPath
                });

        _ontapHttpClientMock
            .Setup(c => c.RenameMaterialAsync(It.IsAny<MaterialRenameArg>()))
            .ThrowsAsync(exception);
    }
}
