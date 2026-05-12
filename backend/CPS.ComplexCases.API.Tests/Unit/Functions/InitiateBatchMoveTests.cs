using System.Net;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CPS.ComplexCases.API.Clients.FileTransfer;
using CPS.ComplexCases.API.Domain.Models;
using CPS.ComplexCases.API.Functions;
using CPS.ComplexCases.API.Services;
using CPS.ComplexCases.API.Tests.Unit.Helpers;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.Common.Services;
using CPS.ComplexCases.Data.Entities;
using CPS.ComplexCases.Data.Models.Requests;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions;

public class InitiateBatchMoveTests
{
    private readonly Mock<ILogger<InitiateBatchMove>> _loggerMock;
    private readonly Mock<IFileTransferClient> _fileTransferClientMock;
    private readonly Mock<IRequestValidator> _requestValidatorMock;
    private readonly Mock<ISecurityGroupMetadataService> _securityGroupMetadataServiceMock;
    private readonly Mock<ICaseMetadataService> _caseMetadataServiceMock;
    private readonly Mock<IInitializationHandler> _initializationHandlerMock;
    private readonly InitiateBatchMove _function;
    private readonly Fixture _fixture;

    private readonly string _testBearerToken;
    private readonly string _testBucketName;
    private readonly Guid _testCorrelationId;
    private readonly string _testUsername;
    private readonly string _testCmsAuthValues;
    private readonly List<SecurityGroup> _defaultSecurityGroups;

    private const string TestNetAppFolder = "CaseRoot";

    public InitiateBatchMoveTests()
    {
        _loggerMock = new Mock<ILogger<InitiateBatchMove>>();
        _fileTransferClientMock = new Mock<IFileTransferClient>();
        _requestValidatorMock = new Mock<IRequestValidator>();
        _securityGroupMetadataServiceMock = new Mock<ISecurityGroupMetadataService>();
        _caseMetadataServiceMock = new Mock<ICaseMetadataService>();
        _initializationHandlerMock = new Mock<IInitializationHandler>();

        _fixture = new Fixture();
        _testBearerToken = _fixture.Create<string>();
        _testBucketName = _fixture.Create<string>();
        _testCorrelationId = _fixture.Create<Guid>();
        _testUsername = _fixture.Create<string>();
        _testCmsAuthValues = _fixture.Create<string>();

        _defaultSecurityGroups = [new() { Id = _fixture.Create<Guid>(), BucketName = _testBucketName, DisplayName = "Test" }];

        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(It.IsAny<string>()))
            .ReturnsAsync(_defaultSecurityGroups);

        _caseMetadataServiceMock
            .Setup(s => s.GetCaseMetadataForCaseIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new CaseMetadata { CaseId = 1, NetappFolderPath = TestNetAppFolder });

        _fileTransferClientMock
            .Setup(c => c.InitiateBatchMoveAsync(It.IsAny<MoveNetAppBatchRequest>(), It.IsAny<Guid>()))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Accepted));

        _function = new InitiateBatchMove(
            _loggerMock.Object,
            _fileTransferClientMock.Object,
            _requestValidatorMock.Object,
            _securityGroupMetadataServiceMock.Object,
            _caseMetadataServiceMock.Object,
            _initializationHandlerMock.Object);
    }

    private FunctionContext CreateFunctionContext() =>
        FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken);

    private void SetupValidRequest(MoveNetAppBatchDto dto)
    {
        _requestValidatorMock
            .Setup(v => v.GetJsonBody<MoveNetAppBatchDto, API.Validators.Requests.MoveNetAppBatchRequestValidator>(It.IsAny<Microsoft.AspNetCore.Http.HttpRequest>()))
            .ReturnsAsync(new ValidatableRequest<MoveNetAppBatchDto> { Value = dto, IsValid = true, ValidationErrors = [] });
    }

    private void SetupInvalidRequest()
    {
        _requestValidatorMock
            .Setup(v => v.GetJsonBody<MoveNetAppBatchDto, API.Validators.Requests.MoveNetAppBatchRequestValidator>(It.IsAny<Microsoft.AspNetCore.Http.HttpRequest>()))
            .ReturnsAsync(new ValidatableRequest<MoveNetAppBatchDto> { Value = null!, IsValid = false, ValidationErrors = ["CaseId must be a positive integer."] });
    }

    private static MoveNetAppBatchDto ValidDto(int caseId = 1) => new()
    {
        CaseId = caseId,
        DestinationPrefix = $"{TestNetAppFolder}/Folder-B/",
        Operations = [new() { Type = NetAppBatchOperationType.Material, SourcePath = $"{TestNetAppFolder}/file.txt" }]
    };

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
    public async Task Run_WhenDestinationPrefixOutsideCaseFolder_DoesNotCallFileTransferClient()
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
        _fileTransferClientMock.Verify(
            c => c.InitiateBatchMoveAsync(It.IsAny<MoveNetAppBatchRequest>(), It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_WithValidRequest_CallsFileTransferClient()
    {
        SetupValidRequest(ValidDto());
        var req = HttpRequestStubHelper.CreateHttpRequest(_testCorrelationId);

        await _function.Run(req, CreateFunctionContext());

        _fileTransferClientMock.Verify(c => c.InitiateBatchMoveAsync(
            It.Is<MoveNetAppBatchRequest>(r =>
                r.CaseId == 1 &&
                r.BearerToken == _testBearerToken &&
                r.BucketName == _testBucketName),
            _testCorrelationId), Times.Once);
    }

    [Fact]
    public async Task Run_WithValidRequest_PassesBucketFromSecurityGroup()
    {
        SetupValidRequest(ValidDto());
        var req = HttpRequestStubHelper.CreateHttpRequest(_testCorrelationId);

        await _function.Run(req, CreateFunctionContext());

        _fileTransferClientMock.Verify(c => c.InitiateBatchMoveAsync(
            It.Is<MoveNetAppBatchRequest>(r => r.BucketName == _testBucketName),
            It.IsAny<Guid>()), Times.Once);
    }

    [Fact]
    public async Task Run_WithValidRequest_MapsOperationsCorrectly()
    {
        var dto = new MoveNetAppBatchDto
        {
            CaseId = 1,
            DestinationPrefix = $"{TestNetAppFolder}/Dest/",
            Operations =
            [
                new() { Type = NetAppBatchOperationType.Material, SourcePath = $"{TestNetAppFolder}/file.pdf" },
                new() { Type = NetAppBatchOperationType.Folder, SourcePath = $"{TestNetAppFolder}/Sub/" },
            ]
        };
        SetupValidRequest(dto);
        var req = HttpRequestStubHelper.CreateHttpRequest(_testCorrelationId);

        await _function.Run(req, CreateFunctionContext());

        _fileTransferClientMock.Verify(c => c.InitiateBatchMoveAsync(
            It.Is<MoveNetAppBatchRequest>(r =>
                r.Operations.Count == 2 &&
                r.Operations.Any(op => op.Type == "Material" && op.SourcePath == $"{TestNetAppFolder}/file.pdf") &&
                r.Operations.Any(op => op.Type == "Folder" && op.SourcePath == $"{TestNetAppFolder}/Sub/")),
            It.IsAny<Guid>()), Times.Once);
    }
}
