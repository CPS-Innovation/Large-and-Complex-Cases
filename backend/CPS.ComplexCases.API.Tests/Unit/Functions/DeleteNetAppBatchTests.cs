using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Amazon.S3;
using AutoFixture;
using CPS.ComplexCases.ActivityLog.Enums;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.API.Domain.Models;
using CPS.ComplexCases.API.Domain.Response;
using CPS.ComplexCases.API.Exceptions;
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
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models;
using CPS.ComplexCases.NetApp.Models.Args;
using Moq;

namespace CPS.ComplexCases.API.Tests.Unit.Functions;

public class DeleteNetAppBatchTests
{
    private readonly Mock<ILogger<DeleteNetAppBatch>> _loggerMock;
    private readonly Mock<INetAppClient> _netAppClientMock;
    private readonly Mock<INetAppArgFactory> _netAppArgFactoryMock;
    private readonly Mock<IActivityLogService> _activityLogServiceMock;
    private readonly Mock<IRequestValidator> _requestValidatorMock;
    private readonly Mock<ISecurityGroupMetadataService> _securityGroupMetadataServiceMock;
    private readonly Mock<ICaseMetadataService> _caseMetadataServiceMock;
    private readonly Mock<IInitializationHandler> _initializationHandlerMock;
    private readonly DeleteNetAppBatch _function;
    private readonly Fixture _fixture;
    private readonly string _testBearerToken;
    private readonly string _testBucketName;
    private readonly Guid _testCorrelationId;
    private readonly string _testUsername;
    private readonly string _testCmsAuthValues;
    private readonly List<SecurityGroup> _defaultSecurityGroups;

    public DeleteNetAppBatchTests()
    {
        _loggerMock = new Mock<ILogger<DeleteNetAppBatch>>();
        _netAppClientMock = new Mock<INetAppClient>();
        _netAppArgFactoryMock = new Mock<INetAppArgFactory>();
        _activityLogServiceMock = new Mock<IActivityLogService>();
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

        _defaultSecurityGroups =
        [
            new() {
                Id = _fixture.Create<Guid>(),
                BucketName = _testBucketName,
                DisplayName = "Test Security Group"
            }
        ];

        _netAppArgFactoryMock
            .Setup(f => f.CreateDeleteFileOrFolderArg(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .Returns((string bearer, string bucket, string op, string path, bool isFolder) =>
                new DeleteFileOrFolderArg { BearerToken = bearer, BucketName = bucket, OperationName = op, Path = path, IsFolder = isFolder });

        _activityLogServiceMock
            .Setup(s => s.CreateActivityLogAsync(It.IsAny<ActionType>(), It.IsAny<ResourceType>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<System.Text.Json.JsonDocument?>()))
            .Returns(Task.CompletedTask);

        _function = new DeleteNetAppBatch(
            _loggerMock.Object,
            _netAppClientMock.Object,
            _netAppArgFactoryMock.Object,
            _activityLogServiceMock.Object,
            _requestValidatorMock.Object,
            _securityGroupMetadataServiceMock.Object,
            _caseMetadataServiceMock.Object,
            _initializationHandlerMock.Object);
    }

    [Fact]
    public async Task Run_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var validationErrors = new List<string> { "Operations cannot be empty." };
        SetupRequestValidator(MakeBatchDto(1, []), isValid: false, errors: validationErrors);

        var (req, ctx) = CreateRequestAndContext();

        // Act
        var result = await _function.Run(req, ctx);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var errors = Assert.IsAssignableFrom<IEnumerable<string>>(badRequest.Value);
        Assert.Equal(validationErrors, errors);

        _caseMetadataServiceMock.Verify(s => s.GetCaseMetadataForCaseIdAsync(It.IsAny<int>()), Times.Never);
        _securityGroupMetadataServiceMock.Verify(s => s.GetUserSecurityGroupsAsync(It.IsAny<string>()), Times.Never);
        _netAppClientMock.Verify(c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()), Times.Never);
    }

    [Fact]
    public async Task Run_MissingCaseMetadata_ReturnsBadRequest()
    {
        // Arrange
        var dto = MakeBatchDto(42, [MaterialOp("CaseRoot/evidence.pdf")]);
        SetupRequestValidator(dto, isValid: true);
        _caseMetadataServiceMock
            .Setup(s => s.GetCaseMetadataForCaseIdAsync(42))
            .ReturnsAsync((CaseMetadata?)null);

        var (req, ctx) = CreateRequestAndContext();

        // Act
        var result = await _function.Run(req, ctx);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("missing", badRequest.Value?.ToString(), StringComparison.OrdinalIgnoreCase);
        _netAppClientMock.Verify(c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()), Times.Never);
    }

    [Fact]
    public async Task Run_EmptyNetAppFolderPath_ReturnsBadRequest()
    {
        // Arrange
        var dto = MakeBatchDto(42, [MaterialOp("CaseRoot/evidence.pdf")]);
        SetupRequestValidator(dto, isValid: true);
        _caseMetadataServiceMock
            .Setup(s => s.GetCaseMetadataForCaseIdAsync(42))
            .ReturnsAsync(new CaseMetadata { CaseId = 42, NetappFolderPath = null });

        var (req, ctx) = CreateRequestAndContext();

        // Act
        var result = await _function.Run(req, ctx);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("missing", badRequest.Value?.ToString(), StringComparison.OrdinalIgnoreCase);
        _netAppClientMock.Verify(c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()), Times.Never);
    }

    [Fact]
    public async Task Run_PathOutsideCaseRoot_RecordsFailureAndSkipsDelete()
    {
        // Arrange
        const string outsidePath = "OtherCase/secret.pdf";
        var dto = MakeBatchDto(1, [MaterialOp(outsidePath)]);
        SetupRequestValidator(dto, isValid: true);
        SetupCaseMetadata();
        SetupSecurityGroups();

        var (req, ctx) = CreateRequestAndContext();

        // Act
        var result = await _function.Run(req, ctx);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DeleteNetAppBatchResponse>(ok.Value);

        Assert.Equal(1, response.TotalRequested);
        Assert.Equal(0, response.Succeeded);
        Assert.Equal(1, response.Failed);
        Assert.Equal("Failed", response.Results[0].Status);
        Assert.NotNull(response.Results[0].Error);

        _netAppClientMock.Verify(c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()), Times.Never);
    }

    [Fact]
    public async Task Run_MixedBatch_ValidAndInvalidPaths_DeletesValidSkipsInvalid()
    {
        // Arrange
        const string validPath = "CaseRoot/evidence.pdf";
        const string invalidPath = "OtherCase/secret.pdf";
        var dto = MakeBatchDto(1, [MaterialOp(validPath), MaterialOp(invalidPath)]);

        SetupRequestValidator(dto, isValid: true);
        SetupCaseMetadata();
        SetupSecurityGroups();
        SetupClientSuccess(validPath, isFolder: false, keysDeleted: 1);

        var (req, ctx) = CreateRequestAndContext();

        // Act
        var result = await _function.Run(req, ctx);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DeleteNetAppBatchResponse>(ok.Value);

        Assert.Equal(2, response.TotalRequested);
        Assert.Equal(1, response.Succeeded);
        Assert.Equal(1, response.Failed);

        Assert.Equal("Deleted", response.Results.Single(r => r.SourcePath == validPath).Status);
        Assert.Equal("Failed", response.Results.Single(r => r.SourcePath == invalidPath).Status);

        _netAppClientMock.Verify(c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()), Times.Once);
    }

    [Fact]
    public async Task Run_PathOutsideCaseRoot_DoesNotLogActivity()
    {
        // Arrange
        const string outsidePath = "OtherCase/secret.pdf";
        var dto = MakeBatchDto(1, [MaterialOp(outsidePath)]);
        SetupRequestValidator(dto, isValid: true);
        SetupCaseMetadata();
        SetupSecurityGroups();

        var (req, ctx) = CreateRequestAndContext();

        // Act
        await _function.Run(req, ctx);

        // Assert
        _activityLogServiceMock.Verify(
            s => s.CreateActivityLogAsync(It.IsAny<ActionType>(), It.IsAny<ResourceType>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<System.Text.Json.JsonDocument?>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_NetAppFolderPathWithoutTrailingSlash_IsNormalizedForPrefixCheck()
    {
        // Arrange — NetappFolderPath stored without trailing slash, path is valid
        const string validPath = "CaseRoot/evidence.pdf";
        var dto = MakeBatchDto(1, [MaterialOp(validPath)]);
        SetupRequestValidator(dto, isValid: true);
        _caseMetadataServiceMock
            .Setup(s => s.GetCaseMetadataForCaseIdAsync(1))
            .ReturnsAsync(new CaseMetadata { CaseId = 1, NetappFolderPath = "CaseRoot" });
        SetupSecurityGroups();
        SetupClientSuccess(validPath, isFolder: false, keysDeleted: 1);

        var (req, ctx) = CreateRequestAndContext();

        // Act
        var result = await _function.Run(req, ctx);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DeleteNetAppBatchResponse>(ok.Value);
        Assert.Equal(1, response.Succeeded);
    }

    [Fact]
    public async Task Run_SingleFileBatch_DeletesFileAndReturnsOk()
    {
        // Arrange
        const int caseId = 42;
        const string path = "CaseRoot/evidence.pdf";
        var dto = MakeBatchDto(caseId, [MaterialOp(path)]);

        SetupRequestValidator(dto, isValid: true);
        SetupCaseMetadata();
        SetupSecurityGroups();
        SetupClientSuccess(path, isFolder: false, keysDeleted: 1);

        var (req, ctx) = CreateRequestAndContext();

        // Act
        var result = await _function.Run(req, ctx);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DeleteNetAppBatchResponse>(ok.Value);

        Assert.Equal(1, response.TotalRequested);
        Assert.Equal(1, response.Succeeded);
        Assert.Equal(0, response.Failed);
        Assert.Single(response.Results);
        Assert.Equal("Deleted", response.Results[0].Status);
        Assert.Equal(path, response.Results[0].SourcePath);
        Assert.Null(response.Results[0].Error);
    }

    [Fact]
    public async Task Run_SingleFileBatch_DoesNotPopulateKeysDeletedForSingleFile()
    {
        // Arrange
        const string path = "CaseRoot/report.pdf";
        var dto = MakeBatchDto(1, [MaterialOp(path)]);

        SetupRequestValidator(dto, isValid: true);
        SetupCaseMetadata();
        SetupSecurityGroups();
        SetupClientSuccess(path, isFolder: false, keysDeleted: 1);

        var (req, ctx) = CreateRequestAndContext();

        // Act
        var result = await _function.Run(req, ctx);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DeleteNetAppBatchResponse>(ok.Value);
        Assert.Null(response.Results[0].KeysDeleted);
    }

    [Fact]
    public async Task Run_SingleFolderBatch_DeletesFolderAndReturnsOk()
    {
        // Arrange
        const int caseId = 99;
        const string path = "CaseRoot/Old-Folder/";
        const int keysDeleted = 47;
        var dto = MakeBatchDto(caseId, [FolderOp(path)]);

        SetupRequestValidator(dto, isValid: true);
        SetupCaseMetadata();
        SetupSecurityGroups();
        SetupClientSuccess(path, isFolder: true, keysDeleted: keysDeleted);

        var (req, ctx) = CreateRequestAndContext();

        // Act
        var result = await _function.Run(req, ctx);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DeleteNetAppBatchResponse>(ok.Value);

        Assert.Equal(1, response.TotalRequested);
        Assert.Equal(1, response.Succeeded);
        Assert.Equal(0, response.Failed);
        Assert.Equal("Deleted", response.Results[0].Status);
        Assert.Equal(keysDeleted, response.Results[0].KeysDeleted);
    }

    [Fact]
    public async Task Run_FolderDelete_PassesIsFolderTrueToArgFactory()
    {
        // Arrange
        const string path = "CaseRoot/Old-Folder/";
        var dto = MakeBatchDto(1, [FolderOp(path)]);

        SetupRequestValidator(dto, isValid: true);
        SetupCaseMetadata();
        SetupSecurityGroups();
        SetupClientSuccess(path, isFolder: true, keysDeleted: 5);

        var (req, ctx) = CreateRequestAndContext();

        // Act
        await _function.Run(req, ctx);

        // Assert
        _netAppArgFactoryMock.Verify(
            f => f.CreateDeleteFileOrFolderArg(_testBearerToken, _testBucketName, string.Empty, path, true),
            Times.Once);
    }

    [Fact]
    public async Task Run_MaterialDelete_PassesIsFolderFalseToArgFactory()
    {
        // Arrange
        const string path = "CaseRoot/evidence.pdf";
        var dto = MakeBatchDto(1, [MaterialOp(path)]);

        SetupRequestValidator(dto, isValid: true);
        SetupCaseMetadata();
        SetupSecurityGroups();
        SetupClientSuccess(path, isFolder: false, keysDeleted: 1);

        var (req, ctx) = CreateRequestAndContext();

        // Act
        await _function.Run(req, ctx);

        // Assert
        _netAppArgFactoryMock.Verify(
            f => f.CreateDeleteFileOrFolderArg(_testBearerToken, _testBucketName, string.Empty, path, false),
            Times.Once);
    }

    [Fact]
    public async Task Run_MixedBatch_DeletesAllItemsAndCountsCorrectly()
    {
        // Arrange
        const string file1 = "CaseRoot/report.pdf";
        const string file2 = "CaseRoot/evidence.docx";
        const string folder = "CaseRoot/Old-Folder/";
        var dto = MakeBatchDto(1, [MaterialOp(file1), MaterialOp(file2), FolderOp(folder)]);

        SetupRequestValidator(dto, isValid: true);
        SetupCaseMetadata();
        SetupSecurityGroups();
        SetupClientSuccess(file1, isFolder: false, keysDeleted: 1);
        SetupClientSuccess(file2, isFolder: false, keysDeleted: 1);
        SetupClientSuccess(folder, isFolder: true, keysDeleted: 10);

        var (req, ctx) = CreateRequestAndContext();

        // Act
        var result = await _function.Run(req, ctx);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DeleteNetAppBatchResponse>(ok.Value);

        Assert.Equal(3, response.TotalRequested);
        Assert.Equal(3, response.Succeeded);
        Assert.Equal(0, response.Failed);
        Assert.All(response.Results, r => Assert.Equal("Deleted", r.Status));
    }

    [Fact]
    public async Task Run_SuccessfulMaterialDelete_LogsSingleBatchDeletedActivity()
    {
        // Arrange
        const int caseId = 42;
        const string path = "CaseRoot/report.pdf";
        var dto = MakeBatchDto(caseId, [MaterialOp(path)]);

        SetupRequestValidator(dto, isValid: true);
        SetupCaseMetadata();
        SetupSecurityGroups();
        SetupClientSuccess(path, isFolder: false, keysDeleted: 1);

        var (req, ctx) = CreateRequestAndContext();

        // Act
        await _function.Run(req, ctx);

        // Assert — one BatchDeleted row per request, regardless of item count
        _activityLogServiceMock.Verify(
            s => s.CreateActivityLogAsync(
                ActionType.MaterialDeleted,
                ResourceType.Material,
                caseId,
                caseId.ToString(),
                null,
                _testUsername,
                It.IsAny<System.Text.Json.JsonDocument?>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_SuccessfulFolderDelete_LogsFolderDeletedActivity()
    {
        // Arrange
        const int caseId = 99;
        const string path = "CaseRoot/Old-Folder/";
        var dto = MakeBatchDto(caseId, [FolderOp(path)]);

        SetupRequestValidator(dto, isValid: true);
        SetupCaseMetadata();
        SetupSecurityGroups();
        SetupClientSuccess(path, isFolder: true, keysDeleted: 5);

        var (req, ctx) = CreateRequestAndContext();

        // Act
        await _function.Run(req, ctx);

        // Assert — folder-only batch uses FolderDeleted action type
        _activityLogServiceMock.Verify(
            s => s.CreateActivityLogAsync(
                ActionType.FolderDeleted,
                ResourceType.Material,
                caseId,
                caseId.ToString(),
                null,
                _testUsername,
                It.IsAny<System.Text.Json.JsonDocument?>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_BatchDelete_DetailsPayloadContainsOutcomeForEachItem()
    {
        // Arrange
        const int caseId = 1;
        const string file = "CaseRoot/report.pdf";
        const string folder = "CaseRoot/Old-Folder/";
        var dto = MakeBatchDto(caseId, [MaterialOp(file), FolderOp(folder)]);

        SetupRequestValidator(dto, isValid: true);
        SetupCaseMetadata();
        SetupSecurityGroups();
        SetupClientSuccess(file, isFolder: false, keysDeleted: 1);
        SetupClientSuccess(folder, isFolder: true, keysDeleted: 3);

        ActionType? capturedActionType = null;
        System.Text.Json.JsonDocument? capturedDetails = null;
        _activityLogServiceMock
            .Setup(s => s.CreateActivityLogAsync(
                It.IsAny<ActionType>(), It.IsAny<ResourceType>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<System.Text.Json.JsonDocument?>()))
            .Callback<ActionType, ResourceType, int, string, string?, string?, System.Text.Json.JsonDocument?>(
                (actionType, _, _, _, _, _, details) => { capturedActionType = actionType; capturedDetails = details; })
            .Returns(Task.CompletedTask);

        var (req, ctx) = CreateRequestAndContext();

        // Act
        await _function.Run(req, ctx);

        // Assert — mixed folder+material batch uses FolderAndMaterialDeleted
        Assert.Equal(ActionType.FolderAndMaterialDeleted, capturedActionType);

        Assert.NotNull(capturedDetails);
        var items = capturedDetails.RootElement.GetProperty("items");
        Assert.Equal(2, items.GetArrayLength());

        var fileItem = items.EnumerateArray().Single(e => e.GetProperty("sourcePath").GetString() == file);
        Assert.Equal("Deleted", fileItem.GetProperty("outcome").GetString());

        var folderItem = items.EnumerateArray().Single(e => e.GetProperty("sourcePath").GetString() == folder);
        Assert.Equal("Deleted", folderItem.GetProperty("outcome").GetString());
        Assert.Equal(3, folderItem.GetProperty("keysDeleted").GetInt32());
    }

    [Fact]
    public async Task Run_PartialFailure_BatchLogDetailsContainsBothSuccessAndFailureOutcomes()
    {
        // Arrange
        const int caseId = 1;
        const string goodFile = "CaseRoot/report.pdf";
        const string badFile = "CaseRoot/corrupted.pdf";
        var dto = MakeBatchDto(caseId, [MaterialOp(goodFile), MaterialOp(badFile)]);

        SetupRequestValidator(dto, isValid: true);
        SetupCaseMetadata();
        SetupSecurityGroups();
        SetupClientSuccess(goodFile, isFolder: false, keysDeleted: 1);
        SetupClientFailure(badFile, isFolder: false, errorMessage: "Delete failed");

        System.Text.Json.JsonDocument? capturedDetails = null;
        _activityLogServiceMock
            .Setup(s => s.CreateActivityLogAsync(
                It.IsAny<ActionType>(), It.IsAny<ResourceType>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<System.Text.Json.JsonDocument?>()))
            .Callback<ActionType, ResourceType, int, string, string?, string?, System.Text.Json.JsonDocument?>(
                (_, _, _, _, _, _, details) => capturedDetails = details)
            .Returns(Task.CompletedTask);

        var (req, ctx) = CreateRequestAndContext();

        // Act
        await _function.Run(req, ctx);

        // Assert — log is written once because at least one item succeeded
        _activityLogServiceMock.Verify(
            s => s.CreateActivityLogAsync(
                ActionType.MaterialDeleted, It.IsAny<ResourceType>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<System.Text.Json.JsonDocument?>()),
            Times.Once);

        Assert.NotNull(capturedDetails);
        var items = capturedDetails.RootElement.GetProperty("items");
        Assert.Equal(2, items.GetArrayLength());

        var successItem = items.EnumerateArray().Single(e => e.GetProperty("sourcePath").GetString() == goodFile);
        Assert.Equal("Deleted", successItem.GetProperty("outcome").GetString());

        var failedItem = items.EnumerateArray().Single(e => e.GetProperty("sourcePath").GetString() == badFile);
        Assert.Equal("Failed", failedItem.GetProperty("outcome").GetString());
    }

    [Fact]
    public async Task Run_ActivityLogThrows_StillReturnsOkAndDoesNotAbortBatch()
    {
        // Arrange
        const string path = "CaseRoot/evidence.pdf";
        var dto = MakeBatchDto(1, [MaterialOp(path)]);

        SetupRequestValidator(dto, isValid: true);
        SetupCaseMetadata();
        SetupSecurityGroups();
        SetupClientSuccess(path, isFolder: false, keysDeleted: 1);

        _activityLogServiceMock
            .Setup(s => s.CreateActivityLogAsync(It.IsAny<ActionType>(), It.IsAny<ResourceType>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<System.Text.Json.JsonDocument?>()))
            .ThrowsAsync(new Exception("Log service unavailable"));

        var (req, ctx) = CreateRequestAndContext();

        // Act
        var result = await _function.Run(req, ctx);

        // Assert — the delete still shows as Deleted despite the log failure
        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DeleteNetAppBatchResponse>(ok.Value);
        Assert.Equal(1, response.Succeeded);
        Assert.Equal("Deleted", response.Results[0].Status);
    }

    [Fact]
    public async Task Run_FailedDelete_DoesNotLogActivity()
    {
        // Arrange
        const string path = "CaseRoot/evidence.pdf";
        var dto = MakeBatchDto(1, [MaterialOp(path)]);

        SetupRequestValidator(dto, isValid: true);
        SetupCaseMetadata();
        SetupSecurityGroups();
        SetupClientFailure(path, isFolder: false, errorMessage: "Something went wrong");

        var (req, ctx) = CreateRequestAndContext();

        // Act
        await _function.Run(req, ctx);

        // Assert
        _activityLogServiceMock.Verify(
            s => s.CreateActivityLogAsync(It.IsAny<ActionType>(), It.IsAny<ResourceType>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<System.Text.Json.JsonDocument?>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_PartialFailure_ContinuesRemainingItemsAndReturnsAll()
    {
        // Arrange
        const string file1 = "CaseRoot/report.pdf";
        const string file2 = "CaseRoot/evidence.docx";
        var dto = MakeBatchDto(1, [MaterialOp(file1), MaterialOp(file2)]);

        SetupRequestValidator(dto, isValid: true);
        SetupCaseMetadata();
        SetupSecurityGroups();
        SetupClientSuccess(file1, isFolder: false, keysDeleted: 1);
        SetupClientFailure(file2, isFolder: false, errorMessage: "Delete failed");

        var (req, ctx) = CreateRequestAndContext();

        // Act
        var result = await _function.Run(req, ctx);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DeleteNetAppBatchResponse>(ok.Value);

        Assert.Equal(2, response.TotalRequested);
        Assert.Equal(1, response.Succeeded);
        Assert.Equal(1, response.Failed);

        var successItem = response.Results.Single(r => r.SourcePath == file1);
        Assert.Equal("Deleted", successItem.Status);

        var failedItem = response.Results.Single(r => r.SourcePath == file2);
        Assert.Equal("Failed", failedItem.Status);
        Assert.NotNull(failedItem.Error);
    }

    [Fact]
    public async Task Run_ClientThrows423_RecordsFailureWithSmbMessageAndContinuesBatch()
    {
        // Arrange
        const string lockedFile = "CaseRoot/open-in-word.docx";
        const string otherFile = "CaseRoot/evidence.pdf";
        var dto = MakeBatchDto(1, [MaterialOp(lockedFile), MaterialOp(otherFile)]);

        SetupRequestValidator(dto, isValid: true);
        SetupCaseMetadata();
        SetupSecurityGroups();

        var lockedEx = new AmazonS3Exception("Locked") { StatusCode = HttpStatusCode.Locked };
        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(It.Is<DeleteFileOrFolderArg>(a => a.Path == lockedFile)))
            .ThrowsAsync(lockedEx);

        SetupClientSuccess(otherFile, isFolder: false, keysDeleted: 1);

        var (req, ctx) = CreateRequestAndContext();

        // Act
        var result = await _function.Run(req, ctx);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DeleteNetAppBatchResponse>(ok.Value);

        Assert.Equal(2, response.TotalRequested);
        Assert.Equal(1, response.Succeeded);
        Assert.Equal(1, response.Failed);

        var failedItem = response.Results.Single(r => r.SourcePath == lockedFile);
        Assert.Equal("Failed", failedItem.Status);
        Assert.Contains("423", failedItem.Error);

        var successItem = response.Results.Single(r => r.SourcePath == otherFile);
        Assert.Equal("Deleted", successItem.Status);
    }

    [Fact]
    public async Task Run_ClientThrows423_DoesNotLogActivity()
    {
        // Arrange
        const string path = "CaseRoot/open-in-word.docx";
        var dto = MakeBatchDto(1, [MaterialOp(path)]);

        SetupRequestValidator(dto, isValid: true);
        SetupCaseMetadata();
        SetupSecurityGroups();

        var lockedEx = new AmazonS3Exception("Locked") { StatusCode = HttpStatusCode.Locked };
        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(It.Is<DeleteFileOrFolderArg>(a => a.Path == path)))
            .ThrowsAsync(lockedEx);

        var (req, ctx) = CreateRequestAndContext();

        // Act
        await _function.Run(req, ctx);

        // Assert
        _activityLogServiceMock.Verify(
            s => s.CreateActivityLogAsync(It.IsAny<ActionType>(), It.IsAny<ResourceType>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<System.Text.Json.JsonDocument?>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_ClientThrowsUnexpectedException_RecordsFailureAndContinuesBatch()
    {
        // Arrange
        const string badFile = "CaseRoot/corrupted.pdf";
        const string goodFile = "CaseRoot/evidence.pdf";
        var dto = MakeBatchDto(1, [MaterialOp(badFile), MaterialOp(goodFile)]);

        SetupRequestValidator(dto, isValid: true);
        SetupCaseMetadata();
        SetupSecurityGroups();

        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(It.Is<DeleteFileOrFolderArg>(a => a.Path == badFile)))
            .ThrowsAsync(new InvalidOperationException("Something broke"));

        SetupClientSuccess(goodFile, isFolder: false, keysDeleted: 1);

        var (req, ctx) = CreateRequestAndContext();

        // Act
        var result = await _function.Run(req, ctx);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DeleteNetAppBatchResponse>(ok.Value);

        Assert.Equal(1, response.Succeeded);
        Assert.Equal(1, response.Failed);

        var failedItem = response.Results.Single(r => r.SourcePath == badFile);
        Assert.Equal("Failed", failedItem.Status);
        Assert.Equal("Something broke", failedItem.Error);
    }

    [Fact]
    public async Task Run_UsesFirstSecurityGroupBucketName()
    {
        // Arrange
        const string path = "CaseRoot/evidence.pdf";
        const string firstBucket = "first-bucket";
        const string secondBucket = "second-bucket";
        var dto = MakeBatchDto(1, [MaterialOp(path)]);

        SetupRequestValidator(dto, isValid: true);
        SetupCaseMetadata();

        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(_testBearerToken))
            .ReturnsAsync(
            [
                new() { Id = _fixture.Create<Guid>(), BucketName = firstBucket, DisplayName = "First" },
                new() { Id = _fixture.Create<Guid>(), BucketName = secondBucket, DisplayName = "Second" }
            ]);

        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()))
            .ReturnsAsync(new DeleteNetAppResult(true, true, 1, null, null));

        var (req, ctx) = CreateRequestAndContext();

        // Act
        await _function.Run(req, ctx);

        // Assert
        _netAppArgFactoryMock.Verify(
            f => f.CreateDeleteFileOrFolderArg(_testBearerToken, firstBucket, string.Empty, path, false),
            Times.Once);
    }

    [Fact]
    public async Task Run_MissingSecurityGroup_ThrowsMissingSecurityGroupException()
    {
        // Arrange
        var dto = MakeBatchDto(1, [MaterialOp("CaseRoot/evidence.pdf")]);
        SetupRequestValidator(dto, isValid: true);
        SetupCaseMetadata();

        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(_testBearerToken))
            .ThrowsAsync(new MissingSecurityGroupException("No security groups found."));

        var (req, ctx) = CreateRequestAndContext();

        // Act & Assert
        await Assert.ThrowsAsync<MissingSecurityGroupException>(() => _function.Run(req, ctx));

        _netAppClientMock.Verify(c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()), Times.Never);
    }

    [Fact]
    public async Task Run_InitializesHandlerWithUsernameAndCorrelationId()
    {
        // Arrange
        var dto = MakeBatchDto(1, [MaterialOp("CaseRoot/evidence.pdf")]);
        SetupRequestValidator(dto, isValid: true);
        SetupCaseMetadata();
        SetupSecurityGroups();
        SetupClientSuccess("CaseRoot/evidence.pdf", isFolder: false, keysDeleted: 1);

        var (req, ctx) = CreateRequestAndContext();

        // Act
        await _function.Run(req, ctx);

        // Assert
        _initializationHandlerMock.Verify(h => h.Initialize(_testUsername, _testCorrelationId, null), Times.Once);
    }

    [Fact]
    public async Task Run_FileNotFound_RecordsNotFoundStatusAndDoesNotCountAsSucceeded()
    {
        // Arrange
        const string missingFile = "CaseRoot/gone.pdf";
        var dto = MakeBatchDto(1, [MaterialOp(missingFile)]);

        SetupRequestValidator(dto, isValid: true);
        SetupCaseMetadata();
        SetupSecurityGroups();
        SetupClientNotFound(missingFile, isFolder: false);

        var (req, ctx) = CreateRequestAndContext();

        // Act
        var result = await _function.Run(req, ctx);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DeleteNetAppBatchResponse>(ok.Value);

        Assert.Equal(1, response.TotalRequested);
        Assert.Equal(0, response.Succeeded);
        Assert.Equal(1, response.NotFound);
        Assert.Equal(0, response.Failed);
        Assert.Equal("NotFound", response.Results[0].Status);
        Assert.Null(response.Results[0].Error);
    }

    [Fact]
    public async Task Run_FileNotFound_LogsActivityWithNotFoundOutcome()
    {
        // Arrange
        const string missingFile = "CaseRoot/gone.pdf";
        var dto = MakeBatchDto(1, [MaterialOp(missingFile)]);

        SetupRequestValidator(dto, isValid: true);
        SetupCaseMetadata();
        SetupSecurityGroups();
        SetupClientNotFound(missingFile, isFolder: false);

        System.Text.Json.JsonDocument? capturedDetails = null;
        _activityLogServiceMock
            .Setup(s => s.CreateActivityLogAsync(
                It.IsAny<ActionType>(), It.IsAny<ResourceType>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<System.Text.Json.JsonDocument?>()))
            .Callback<ActionType, ResourceType, int, string, string?, string?, System.Text.Json.JsonDocument?>(
                (_, _, _, _, _, _, details) => capturedDetails = details)
            .Returns(Task.CompletedTask);

        var (req, ctx) = CreateRequestAndContext();

        // Act
        await _function.Run(req, ctx);

        // Assert — NotFound attempts are auditable even though nothing was physically deleted
        _activityLogServiceMock.Verify(
            s => s.CreateActivityLogAsync(
                ActionType.MaterialDeleted, It.IsAny<ResourceType>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<System.Text.Json.JsonDocument?>()),
            Times.Once);

        Assert.NotNull(capturedDetails);
        var items = capturedDetails.RootElement.GetProperty("items");
        var item = items.EnumerateArray().Single();
        Assert.Equal(missingFile, item.GetProperty("sourcePath").GetString());
        Assert.Equal("NotFound", item.GetProperty("outcome").GetString());
    }

    [Fact]
    public async Task Run_MixedBatch_NotFoundAndDeleted_CountsCorrectlyAndLogsActivity()
    {
        // Arrange
        const string existingFile = "CaseRoot/evidence.pdf";
        const string missingFile = "CaseRoot/gone.pdf";
        var dto = MakeBatchDto(1, [MaterialOp(existingFile), MaterialOp(missingFile)]);

        SetupRequestValidator(dto, isValid: true);
        SetupCaseMetadata();
        SetupSecurityGroups();
        SetupClientSuccess(existingFile, isFolder: false, keysDeleted: 1);
        SetupClientNotFound(missingFile, isFolder: false);

        var (req, ctx) = CreateRequestAndContext();

        // Act
        var result = await _function.Run(req, ctx);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DeleteNetAppBatchResponse>(ok.Value);

        Assert.Equal(2, response.TotalRequested);
        Assert.Equal(1, response.Succeeded);
        Assert.Equal(1, response.NotFound);
        Assert.Equal(0, response.Failed);

        Assert.Equal("Deleted", response.Results.Single(r => r.SourcePath == existingFile).Status);
        Assert.Equal("NotFound", response.Results.Single(r => r.SourcePath == missingFile).Status);

        // Activity log is written because at least one item was actually deleted
        _activityLogServiceMock.Verify(
            s => s.CreateActivityLogAsync(
                ActionType.MaterialDeleted, It.IsAny<ResourceType>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<System.Text.Json.JsonDocument?>()),
            Times.Once);
    }

    private static DeleteNetAppBatchDto MakeBatchDto(int caseId, List<DeleteNetAppBatchOperationDto> operations) =>
        new() { CaseId = caseId, Operations = operations };

    private static DeleteNetAppBatchOperationDto MaterialOp(string sourcePath) =>
        new() { Type = NetAppDeleteOperationType.Material, SourcePath = sourcePath };

    private static DeleteNetAppBatchOperationDto FolderOp(string sourcePath) =>
        new() { Type = NetAppDeleteOperationType.Folder, SourcePath = sourcePath };

    private void SetupRequestValidator(DeleteNetAppBatchDto dto, bool isValid, List<string>? errors = null)
    {
        _requestValidatorMock
            .Setup(x => x.GetJsonBody<DeleteNetAppBatchDto, DeleteNetAppBatchRequestValidator>(It.IsAny<HttpRequest>()))
            .ReturnsAsync(new ValidatableRequest<DeleteNetAppBatchDto>
            {
                IsValid = isValid,
                ValidationErrors = errors ?? [],
                Value = dto
            });
    }

    private void SetupCaseMetadata(string netappFolderPath = "CaseRoot/")
    {
        _caseMetadataServiceMock
            .Setup(s => s.GetCaseMetadataForCaseIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new CaseMetadata { CaseId = 1, NetappFolderPath = netappFolderPath });
    }

    private void SetupSecurityGroups()
    {
        _securityGroupMetadataServiceMock
            .Setup(s => s.GetUserSecurityGroupsAsync(_testBearerToken))
            .ReturnsAsync(_defaultSecurityGroups);
    }

    private void SetupClientSuccess(string path, bool isFolder, int keysDeleted)
    {
        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(It.Is<DeleteFileOrFolderArg>(a => a.Path == path && a.IsFolder == isFolder)))
            .ReturnsAsync(new DeleteNetAppResult(true, true, keysDeleted, null, null));
    }

    private void SetupClientNotFound(string path, bool isFolder)
    {
        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(It.Is<DeleteFileOrFolderArg>(a => a.Path == path && a.IsFolder == isFolder)))
            .ReturnsAsync(new DeleteNetAppResult(true, false, 0, null, null));
    }

    private void SetupClientFailure(string path, bool isFolder, string errorMessage)
    {
        _netAppClientMock
            .Setup(c => c.DeleteFileOrFolderAsync(It.Is<DeleteFileOrFolderArg>(a => a.Path == path && a.IsFolder == isFolder)))
            .ReturnsAsync(new DeleteNetAppResult(false, false, 0, errorMessage, null));
    }

    private (HttpRequest req, FunctionContext ctx) CreateRequestAndContext() =>
        (HttpRequestStubHelper.CreateHttpRequest(_testCorrelationId),
         FunctionContextStubHelper.CreateFunctionContextStub(_testCorrelationId, _testCmsAuthValues, _testUsername, _testBearerToken));
}
