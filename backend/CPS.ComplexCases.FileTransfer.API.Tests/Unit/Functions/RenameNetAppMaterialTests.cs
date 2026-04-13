using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Amazon.S3;
using CPS.ComplexCases.ActivityLog.Enums;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.Common.Constants;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Helpers;
using CPS.ComplexCases.Common.Models;
using CPS.ComplexCases.FileTransfer.API.Durable.Activity;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Functions;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.FileTransfer.API.Validators;
using CPS.ComplexCases.NetApp.Client;
using CPS.ComplexCases.NetApp.Factories;
using CPS.ComplexCases.NetApp.Models.Args;
using Moq;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Functions;

public class RenameNetAppMaterialTests
{
    private readonly Mock<ILogger<RenameNetAppMaterial>> _loggerMock;
    private readonly Mock<IRequestValidator> _requestValidatorMock;
    private readonly Mock<IInitializationHandler> _initializationHandlerMock;
    private readonly Mock<ITransferFile> _transferFileMock;
    private readonly Mock<INetAppClient> _netAppClientMock;
    private readonly Mock<INetAppArgFactory> _netAppArgFactoryMock;
    private readonly Mock<IActivityLogService> _activityLogServiceMock;
    private readonly RenameNetAppMaterial _function;
    private const string SourcePath = "materials/case42/document.pdf";
    private const string DestinationPath = "materials/case42/renamed-document.pdf";
    private const string BearerToken = "test-bearer-token";
    private const string BucketName = "flexgroup4";

    public RenameNetAppMaterialTests()
    {
        _loggerMock = new Mock<ILogger<RenameNetAppMaterial>>();
        _requestValidatorMock = new Mock<IRequestValidator>();
        _initializationHandlerMock = new Mock<IInitializationHandler>();
        _transferFileMock = new Mock<ITransferFile>();
        _netAppClientMock = new Mock<INetAppClient>();
        _netAppArgFactoryMock = new Mock<INetAppArgFactory>();
        _activityLogServiceMock = new Mock<IActivityLogService>();

        _netAppArgFactoryMock
            .Setup(f => f.CreateGetObjectArg(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()))
            .Returns((string bearer, string bucket, string key, string? etag) =>
                new GetObjectArg { BearerToken = bearer, BucketName = bucket, ObjectKey = key });

        _netAppArgFactoryMock
            .Setup(f => f.CreateDeleteFileOrFolderArg(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string bearer, string bucket, string op, string path) =>
                new DeleteFileOrFolderArg { BearerToken = bearer, BucketName = bucket, OperationName = op, Path = path });

        _function = new RenameNetAppMaterial(
            _loggerMock.Object,
            _requestValidatorMock.Object,
            _initializationHandlerMock.Object,
            _transferFileMock.Object,
            _netAppClientMock.Object,
            _netAppArgFactoryMock.Object,
            _activityLogServiceMock.Object);
    }

    [Fact]
    public async Task Run_InvalidRequest_ReturnsBadRequest()
    {
        var errors = new List<string> { "SourcePath is required." };
        SetupRequestValidator(CreateValidRequest(), isValid: false, errors: errors);

        var result = await _function.Run(CreateHttpRequest());

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(errors, bad.Value);
    }

    [Fact]
    public async Task Run_SourceDoesNotExist_Returns404()
    {
        SetupRequestValidator(CreateValidRequest(), isValid: true);
        _netAppClientMock.Setup(c => c.DoesObjectExistAsync(It.Is<GetObjectArg>(a => a.ObjectKey == SourcePath)))
            .ReturnsAsync(false);

        var result = await _function.Run(CreateHttpRequest());

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains(SourcePath, notFound.Value!.ToString());
        _transferFileMock.Verify(t => t.Run(It.IsAny<TransferFilePayload>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Run_DestinationAlreadyExists_Returns409()
    {
        SetupRequestValidator(CreateValidRequest(), isValid: true);
        _netAppClientMock.Setup(c => c.DoesObjectExistAsync(It.Is<GetObjectArg>(a => a.ObjectKey == SourcePath)))
            .ReturnsAsync(true);
        _netAppClientMock.Setup(c => c.DoesObjectExistAsync(It.Is<GetObjectArg>(a => a.ObjectKey == DestinationPath)))
            .ReturnsAsync(true);

        var result = await _function.Run(CreateHttpRequest());

        var conflict = Assert.IsType<ConflictObjectResult>(result);
        Assert.Contains(DestinationPath, conflict.Value!.ToString());
        _transferFileMock.Verify(t => t.Run(It.IsAny<TransferFilePayload>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Run_CopyFails_AttemptsDestinationCleanupAndReturns500()
    {
        SetupRequestValidator(CreateValidRequest(), isValid: true);
        SetupSourceExists();
        SetupDestinationMissing();

        _transferFileMock.Setup(t => t.Run(It.IsAny<TransferFilePayload>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TransferResult
            {
                IsSuccess = false,
                FailedItem = new TransferFailedItem
                {
                    SourcePath = SourcePath,
                    ErrorCode = TransferErrorCode.IntegrityVerificationFailed,
                    ErrorMessage = "Upload completed but failed to verify."
                }
            });

        _netAppClientMock.Setup(c => c.DeleteFileOrFolderAsync(It.Is<DeleteFileOrFolderArg>(a => a.Path == DestinationPath)))
            .ReturnsAsync("cleaned up");

        var result = await _function.Run(CreateHttpRequest());

        Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, ((ObjectResult)result).StatusCode);

        _netAppClientMock.Verify(
            c => c.DeleteFileOrFolderAsync(It.Is<DeleteFileOrFolderArg>(a => a.Path == DestinationPath)),
            Times.Once);

        _netAppClientMock.Verify(
            c => c.DeleteFileOrFolderAsync(It.Is<DeleteFileOrFolderArg>(a => a.Path == SourcePath)),
            Times.Never);

        _activityLogServiceMock.Verify(
            s => s.CreateActivityLogAsync(It.IsAny<ActionType>(), It.IsAny<ResourceType>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<System.Text.Json.JsonDocument?>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_SuccessfulRenameSmallFile_DeletesSourceAndLogsActivity()
    {
        SetupRequestValidator(CreateValidRequest(), isValid: true);
        SetupSourceExists();
        SetupDestinationMissing();
        SetupSuccessfulTransfer();

        _netAppClientMock.Setup(c => c.DeleteFileOrFolderAsync(It.Is<DeleteFileOrFolderArg>(a => a.Path == SourcePath)))
            .ReturnsAsync("deleted");

        _activityLogServiceMock
            .Setup(s => s.CreateActivityLogAsync(
                ActionType.MaterialRenamed, ResourceType.Material, It.IsAny<int>(),
                SourcePath, It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<System.Text.Json.JsonDocument?>()))
            .Returns(Task.CompletedTask);

        var result = await _function.Run(CreateHttpRequest());

        Assert.IsType<OkObjectResult>(result);

        _netAppClientMock.Verify(
            c => c.DeleteFileOrFolderAsync(It.Is<DeleteFileOrFolderArg>(a => a.Path == SourcePath)),
            Times.Once);

        _activityLogServiceMock.Verify(
            s => s.CreateActivityLogAsync(ActionType.MaterialRenamed, ResourceType.Material, 42,
                SourcePath, It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<System.Text.Json.JsonDocument?>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_TransferFilePayload_UsesNetAppToNetAppDirection()
    {
        TransferFilePayload? capturedPayload = null;

        SetupRequestValidator(CreateValidRequest(), isValid: true);
        SetupSourceExists();
        SetupDestinationMissing();

        _transferFileMock
            .Setup(t => t.Run(It.IsAny<TransferFilePayload>(), It.IsAny<CancellationToken>()))
            .Callback<TransferFilePayload, CancellationToken>((p, _) => capturedPayload = p)
            .ReturnsAsync(new TransferResult { IsSuccess = true });

        _netAppClientMock.Setup(c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()))
            .ReturnsAsync("deleted");

        await _function.Run(CreateHttpRequest());

        Assert.NotNull(capturedPayload);
        Assert.Equal(CPS.ComplexCases.Common.Models.Domain.Enums.TransferDirection.NetAppToNetApp, capturedPayload!.TransferDirection);
        Assert.Equal(SourcePath, capturedPayload.SourcePath.Path);
        Assert.Equal("renamed-document.pdf", capturedPayload.SourcePath.ModifiedPath);
        Assert.Equal("materials/case42/", capturedPayload.DestinationPath);
        Assert.Equal(BearerToken, capturedPayload.BearerToken);
        Assert.Equal(BucketName, capturedPayload.BucketName);
    }

    [Fact]
    public async Task Run_DeleteReturns423_Returns409WithCloseFileMessage()
    {
        SetupRequestValidator(CreateValidRequest(), isValid: true);
        SetupSourceExists();
        SetupDestinationMissing();
        SetupSuccessfulTransfer();

        var lockedEx = new AmazonS3Exception("Locked") { StatusCode = HttpStatusCode.Locked };
        _netAppClientMock.Setup(c => c.DeleteFileOrFolderAsync(It.Is<DeleteFileOrFolderArg>(a => a.Path == SourcePath)))
            .ThrowsAsync(lockedEx);

        var result = await _function.Run(CreateHttpRequest());

        var conflict = Assert.IsType<ConflictObjectResult>(result);
        Assert.Contains("close it and try again", conflict.Value!.ToString());

        _activityLogServiceMock.Verify(
            s => s.CreateActivityLogAsync(It.IsAny<ActionType>(), It.IsAny<ResourceType>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<System.Text.Json.JsonDocument?>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_DeleteFailsWithUnexpectedException_Returns500()
    {
        SetupRequestValidator(CreateValidRequest(), isValid: true);
        SetupSourceExists();
        SetupDestinationMissing();
        SetupSuccessfulTransfer();

        _netAppClientMock.Setup(c => c.DeleteFileOrFolderAsync(It.Is<DeleteFileOrFolderArg>(a => a.Path == SourcePath)))
            .ThrowsAsync(new InvalidOperationException("Something broke"));

        var result = await _function.Run(CreateHttpRequest());

        var serverError = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, serverError.StatusCode);
    }

    [Fact]
    public async Task Run_ActivityLog_RecordsMaterialRenamedWithCorrectPaths()
    {
        System.Text.Json.JsonDocument? capturedDetails = null;

        SetupRequestValidator(CreateValidRequest(), isValid: true);
        SetupSourceExists();
        SetupDestinationMissing();
        SetupSuccessfulTransfer();

        _netAppClientMock.Setup(c => c.DeleteFileOrFolderAsync(It.IsAny<DeleteFileOrFolderArg>()))
            .ReturnsAsync("deleted");

        _activityLogServiceMock
            .Setup(s => s.CreateActivityLogAsync(
                It.IsAny<ActionType>(), It.IsAny<ResourceType>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(),
                It.IsAny<System.Text.Json.JsonDocument?>()))
            .Callback<ActionType, ResourceType, int, string, string?, string?, System.Text.Json.JsonDocument?>(
                (_, _, _, _, _, _, details) => capturedDetails = details)
            .Returns(Task.CompletedTask);

        await _function.Run(CreateHttpRequest());

        _activityLogServiceMock.Verify(
            s => s.CreateActivityLogAsync(
                ActionType.MaterialRenamed,
                ResourceType.Material,
                42,
                SourcePath,
                "renamed-document.pdf",
                "user@example.com",
                It.IsAny<System.Text.Json.JsonDocument?>()),
            Times.Once);

        Assert.NotNull(capturedDetails);
        var root = capturedDetails!.RootElement;
        Assert.Equal(SourcePath, root.GetProperty("sourcePath").GetString());
        Assert.Equal(DestinationPath, root.GetProperty("destinationPath").GetString());
    }

    private void SetupRequestValidator(RenameNetAppMaterialRequest request, bool isValid, List<string>? errors = null)
    {
        _requestValidatorMock
            .Setup(v => v.GetJsonBody<RenameNetAppMaterialRequest, RenameNetAppMaterialValidator>(It.IsAny<HttpRequest>()))
            .ReturnsAsync(new ValidatableRequest<RenameNetAppMaterialRequest>
            {
                Value = request,
                IsValid = isValid,
                ValidationErrors = errors ?? new List<string>()
            });
    }

    private void SetupSourceExists() =>
        _netAppClientMock.Setup(c => c.DoesObjectExistAsync(It.Is<GetObjectArg>(a => a.ObjectKey == SourcePath)))
            .ReturnsAsync(true);

    private void SetupDestinationMissing() =>
        _netAppClientMock.Setup(c => c.DoesObjectExistAsync(It.Is<GetObjectArg>(a => a.ObjectKey == DestinationPath)))
            .ReturnsAsync(false);

    private void SetupSuccessfulTransfer() =>
        _transferFileMock.Setup(t => t.Run(It.IsAny<TransferFilePayload>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TransferResult { IsSuccess = true });

    private static RenameNetAppMaterialRequest CreateValidRequest() =>
        new()
        {
            CaseId = 42,
            SourcePath = SourcePath,
            DestinationPath = DestinationPath,
            BearerToken = BearerToken,
            BucketName = BucketName,
            Username = "user@example.com"
        };

    private static HttpRequest CreateHttpRequest()
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Headers[HttpHeaderKeys.CorrelationId] = Guid.NewGuid().ToString();
        return request;
    }
}
