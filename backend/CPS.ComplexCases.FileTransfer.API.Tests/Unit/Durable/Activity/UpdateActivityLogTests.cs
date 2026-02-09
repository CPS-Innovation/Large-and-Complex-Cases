using Microsoft.DurableTask.Client.Entities;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging;
using AutoFixture;
using AutoFixture.AutoMoq;
using CPS.ComplexCases.ActivityLog.Enums;
using CPS.ComplexCases.ActivityLog.Services;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.FileTransfer.API.Durable.Activity;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using CPS.ComplexCases.FileTransfer.API.Tests.Unit.Stubs;
using Moq;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Durable.Activity;

public class UpdateActivityLogTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IActivityLogService> _activityLogServiceMock;
    private readonly Mock<ILogger<UpdateActivityLog>> _loggerMock;
    private readonly Mock<IInitializationHandler> _initializationHandlerMock;
    private readonly DurableEntityClientStub _durableEntityClientStub;
    private readonly DurableTaskClientStub _durableTaskClientStub;
    private readonly UpdateActivityLog _activity;
    private readonly string _bearerToken;

    public UpdateActivityLogTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());

        _activityLogServiceMock = new Mock<IActivityLogService>();
        _loggerMock = new Mock<ILogger<UpdateActivityLog>>();
        _initializationHandlerMock = new Mock<IInitializationHandler>();

        _durableEntityClientStub = new DurableEntityClientStub("TestClient");
        _durableTaskClientStub = new DurableTaskClientStub(_durableEntityClientStub);

        _activity = new UpdateActivityLog(_activityLogServiceMock.Object, _loggerMock.Object, _initializationHandlerMock.Object);
        _bearerToken = _fixture.Create<string>();
    }

    [Fact]
    public async Task Run_ThrowsInvalidOperationException_WhenEntityNotFound()
    {
        // Arrange
        var payload = new UpdateActivityLogPayload
        {
            TransferId = Guid.NewGuid().ToString(),
            ActionType = ActionType.TransferCompleted,
            UserName = _fixture.Create<string>()
        };

        _durableEntityClientStub.OnGetEntityAsync = (id, token) =>
        {
            return Task.FromResult<EntityMetadata<TransferEntity>?>(null);
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _activity.Run(payload, _durableTaskClientStub));

        Assert.Contains($"Transfer entity with ID {payload.TransferId} not found", exception.Message);
    }
    [Fact]
    public async Task Run_ThrowsArgumentNullException_WhenPayloadIsNull()
    {
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _activity.Run(null!, _durableTaskClientStub));

        Assert.Equal("payload", exception.ParamName);
    }

    [Fact]
    public async Task Run_CreatesActivityLog_WithSuccessAndFailedItems()
    {
        // Arrange
        var transferId = _fixture.Create<Guid>();
        var userName = _fixture.Create<string>();
        var caseId = _fixture.Create<int>();

        var entityState = new TransferEntity
        {
            Id = transferId,
            CaseId = caseId,
            Direction = TransferDirection.EgressToNetApp,
            TransferType = TransferType.Copy,
            TotalFiles = 2,
            DestinationPath = "/dest/path",
            BearerToken = _bearerToken,
            SourcePaths =
            [
                new TransferSourcePath
            {
                FullFilePath = @"C:\source\file1.txt",
                Path = "/source"
            }
            ],
            SuccessfulItems =
            [
                new TransferItem
            {
                SourcePath = "/source/file1.txt",
                Size = 2048,
                IsRenamed = false,
                Status = TransferItemStatus.Completed
            }
            ],
            FailedItems =
            [
                new TransferFailedItem
            {
                SourcePath = "/source/file2.txt",
                ErrorCode = TransferErrorCode.GeneralError,
                ErrorMessage = "Connection reset"
            }
            ]
        };

        _durableEntityClientStub.OnGetEntityAsync = (id, token) =>
            Task.FromResult<EntityMetadata<TransferEntity>?>(new EntityMetadata<TransferEntity>(
                new Microsoft.DurableTask.Entities.EntityInstanceId("TransferEntity", transferId.ToString()),
                entityState
            ));

        var payload = new UpdateActivityLogPayload
        {
            TransferId = transferId.ToString(),
            ActionType = ActionType.TransferCompleted,
            UserName = userName
        };

        // Act
        await _activity.Run(payload, _durableTaskClientStub);

        // Assert
        _activityLogServiceMock.Verify(service => service.CreateActivityLogAsync(
            payload.ActionType,
            ResourceType.FileTransfer,
            caseId,
            entityState.Id.ToString(),
            entityState.Direction.ToString(),
            userName,
            It.IsAny<System.Text.Json.JsonDocument>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_ThrowsInvalidOperationException_WhenSourcePathIsNull()
    {
        // Arrange
        var transferId = Guid.NewGuid();
        var entityState = new TransferEntity
        {
            Id = transferId,
            CaseId = 1,
            Direction = TransferDirection.EgressToNetApp,
            TransferType = TransferType.Copy,
            TotalFiles = 1,
            BearerToken = _bearerToken,
            SourcePaths = [new TransferSourcePath
            {
                FullFilePath = null!,
                Path = null!
            }],
            DestinationPath = "/dest/path",
        };

        _durableEntityClientStub.OnGetEntityAsync = (_, _) =>
            Task.FromResult<EntityMetadata<TransferEntity>?>(new EntityMetadata<TransferEntity>(
                new EntityInstanceId("TransferEntity", transferId.ToString()),
                entityState
            ));

        var payload = new UpdateActivityLogPayload
        {
            TransferId = transferId.ToString(),
            ActionType = ActionType.TransferCompleted,
            UserName = "user"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _activity.Run(payload, _durableTaskClientStub));
        Assert.Equal("Source path cannot be null or empty.", ex.Message);
    }


    [Fact]
    public async Task Run_IncludesDeletionErrors_WhenTransferTypeIsMoveAndDirectionIsEgressToNetApp()
    {
        // Arrange
        var transferId = Guid.NewGuid();
        var userName = _fixture.Create<string>();
        var caseId = _fixture.Create<int>();

        var entityState = new TransferEntity
        {
            Id = transferId,
            CaseId = caseId,
            Direction = TransferDirection.EgressToNetApp,
            TransferType = TransferType.Move,
            TotalFiles = 1,
            BearerToken = _bearerToken,
            SourcePaths = [new TransferSourcePath { FullFilePath = @"C:\source\file.txt", Path = "/source" }],
            DeletionErrors = [new DeletionError
            {
                FileId = _fixture.Create<string>(),
                ErrorMessage = "Access denied"
            }],
            DestinationPath = "/dest/path",
        };

        _durableEntityClientStub.OnGetEntityAsync = (_, _) =>
            Task.FromResult<EntityMetadata<TransferEntity>?>(new EntityMetadata<TransferEntity>(
                new EntityInstanceId("TransferEntity", transferId.ToString()),
                entityState
            ));

        var payload = new UpdateActivityLogPayload
        {
            TransferId = transferId.ToString(),
            ActionType = ActionType.TransferCompleted,
            UserName = userName
        };

        // Act
        await _activity.Run(payload, _durableTaskClientStub);

        // Assert
        _activityLogServiceMock.Verify(service => service.CreateActivityLogAsync(
            payload.ActionType,
            ResourceType.FileTransfer,
            caseId,
            entityState.Id.ToString(),
            entityState.Direction.ToString(),
            userName,
            It.Is<System.Text.Json.JsonDocument>(doc => doc.RootElement.ToString().Contains("Access denied"))
        ), Times.Once);
    }

    [Fact]
    public async Task Run_DoesNotIncludeDeletionErrors_WhenTransferTypeIsCopy()
    {
        // Arrange
        var transferId = Guid.NewGuid();
        var userName = _fixture.Create<string>();

        var entityState = new TransferEntity
        {
            Id = transferId,
            CaseId = 123,
            Direction = TransferDirection.EgressToNetApp,
            TransferType = TransferType.Copy,
            TotalFiles = 1,
            BearerToken = _bearerToken,
            SourcePaths = [new TransferSourcePath { FullFilePath = @"C:\test.txt", Path = "/source" }],
            DeletionErrors = [new DeletionError { FileId = "/source/test.txt", ErrorMessage = "Should be ignored" }],
            DestinationPath = "/dest/path",
        };

        _durableEntityClientStub.OnGetEntityAsync = (_, _) =>
            Task.FromResult<EntityMetadata<TransferEntity>?>(new EntityMetadata<TransferEntity>(
                new EntityInstanceId("TransferEntity", transferId.ToString()),
                entityState
            ));

        var payload = new UpdateActivityLogPayload
        {
            TransferId = transferId.ToString(),
            ActionType = ActionType.TransferCompleted,
            UserName = userName
        };

        // Act
        await _activity.Run(payload, _durableTaskClientStub);

        // Assert
        _activityLogServiceMock.Verify(service => service.CreateActivityLogAsync(
            payload.ActionType,
            ResourceType.FileTransfer,
            entityState.CaseId,
            entityState.Id.ToString(),
            entityState.Direction.ToString(),
            userName,
            It.Is<System.Text.Json.JsonDocument>(doc =>
                !doc.RootElement.ToString().Contains("Should be ignored"))
        ), Times.Once);
    }

    [Fact]
    public async Task Run_TransferCompleted_IncludesEndTime_WhenCompletedAtIsSet()
    {
        // Arrange
        var transferId = Guid.NewGuid();
        var userName = _fixture.Create<string>();
        var caseId = _fixture.Create<int>();
        var startedAt = DateTime.UtcNow.AddMinutes(-5);
        var completedAt = DateTime.UtcNow;

        var entityState = new TransferEntity
        {
            Id = transferId,
            CaseId = caseId,
            Direction = TransferDirection.EgressToNetApp,
            TransferType = TransferType.Copy,
            TotalFiles = 1,
            BearerToken = _bearerToken,
            SourcePaths = [new TransferSourcePath { FullFilePath = @"C:\source\file.txt", Path = "/source" }],
            DestinationPath = "/dest/path",
            StartedAt = startedAt,
            CompletedAt = completedAt,
            SuccessfulItems =
            [
                new TransferItem
                {
                    SourcePath = "/source/file.txt",
                    Size = 1024,
                    IsRenamed = false,
                    Status = TransferItemStatus.Completed
                }
            ]
        };

        _durableEntityClientStub.OnGetEntityAsync = (_, _) =>
            Task.FromResult<EntityMetadata<TransferEntity>?>(new EntityMetadata<TransferEntity>(
                new EntityInstanceId("TransferEntity", transferId.ToString()),
                entityState
            ));

        var payload = new UpdateActivityLogPayload
        {
            TransferId = transferId.ToString(),
            ActionType = ActionType.TransferCompleted,
            UserName = userName
        };

        // Act
        await _activity.Run(payload, _durableTaskClientStub);

        // Assert
        _activityLogServiceMock.Verify(service => service.CreateActivityLogAsync(
            payload.ActionType,
            ResourceType.FileTransfer,
            caseId,
            entityState.Id.ToString(),
            entityState.Direction.ToString(),
            userName,
            It.Is<System.Text.Json.JsonDocument>(doc =>
                doc.RootElement.GetProperty("endTime").ValueKind != System.Text.Json.JsonValueKind.Null)),
            Times.Once);
    }

    [Fact]
    public async Task Run_UsesSourceRootFolderPath_WhenSet()
    {
        // Arrange
        var transferId = Guid.NewGuid();
        var userName = _fixture.Create<string>();
        var caseId = _fixture.Create<int>();
        const string sourceRootFolderPath = "egress/folder1";

        var entityState = new TransferEntity
        {
            Id = transferId,
            CaseId = caseId,
            Direction = TransferDirection.EgressToNetApp,
            TransferType = TransferType.Copy,
            TotalFiles = 2,
            BearerToken = _bearerToken,
            SourceRootFolderPath = sourceRootFolderPath,
            SourcePaths =
            [
                new TransferSourcePath { FullFilePath = @"C:\egress\folder1\folder1.1\file1.txt", Path = "egress/folder1/folder1.1/file1.txt" },
                new TransferSourcePath { FullFilePath = @"C:\egress\folder1\folder1.2\file2.txt", Path = "egress/folder1/folder1.2/file2.txt" }
            ],
            DestinationPath = "/dest/path",
            SuccessfulItems =
            [
                new TransferItem { SourcePath = "egress/folder1/folder1.1/file1.txt", Size = 100, IsRenamed = false, Status = TransferItemStatus.Completed },
                new TransferItem { SourcePath = "egress/folder1/folder1.2/file2.txt", Size = 200, IsRenamed = false, Status = TransferItemStatus.Completed }
            ]
        };

        _durableEntityClientStub.OnGetEntityAsync = (_, _) =>
            Task.FromResult<EntityMetadata<TransferEntity>?>(new EntityMetadata<TransferEntity>(
                new EntityInstanceId("TransferEntity", transferId.ToString()),
                entityState
            ));

        var payload = new UpdateActivityLogPayload
        {
            TransferId = transferId.ToString(),
            ActionType = ActionType.TransferCompleted,
            UserName = userName
        };

        // Act
        await _activity.Run(payload, _durableTaskClientStub);

        // Assert
        _activityLogServiceMock.Verify(service => service.CreateActivityLogAsync(
            payload.ActionType,
            ResourceType.FileTransfer,
            caseId,
            entityState.Id.ToString(),
            entityState.Direction.ToString(),
            userName,
            It.Is<System.Text.Json.JsonDocument>(doc =>
                doc.RootElement.GetProperty("sourcePath").GetString() == sourceRootFolderPath)),
            Times.Once);
    }

    [Fact]
    public async Task Run_UsesFirstSourcePathAsFallback_WhenSourceRootFolderPathIsNull()
    {
        // Arrange
        var transferId = Guid.NewGuid();
        var userName = _fixture.Create<string>();
        var caseId = _fixture.Create<int>();
        const string expectedPath = "C:/source";

        var entityState = new TransferEntity
        {
            Id = transferId,
            CaseId = caseId,
            Direction = TransferDirection.EgressToNetApp,
            TransferType = TransferType.Copy,
            TotalFiles = 1,
            BearerToken = _bearerToken,
            SourceRootFolderPath = null,
            SourcePaths = [new TransferSourcePath { FullFilePath = @"C:\source\file.txt", Path = "/source" }],
            DestinationPath = "/dest/path"
        };

        _durableEntityClientStub.OnGetEntityAsync = (_, _) =>
            Task.FromResult<EntityMetadata<TransferEntity>?>(new EntityMetadata<TransferEntity>(
                new EntityInstanceId("TransferEntity", transferId.ToString()),
                entityState
            ));

        var payload = new UpdateActivityLogPayload
        {
            TransferId = transferId.ToString(),
            ActionType = ActionType.TransferCompleted,
            UserName = userName
        };

        // Act
        await _activity.Run(payload, _durableTaskClientStub);

        // Assert
        _activityLogServiceMock.Verify(service => service.CreateActivityLogAsync(
            payload.ActionType,
            ResourceType.FileTransfer,
            caseId,
            entityState.Id.ToString(),
            entityState.Direction.ToString(),
            userName,
            It.Is<System.Text.Json.JsonDocument>(doc =>
                doc.RootElement.GetProperty("sourcePath").GetString() == expectedPath)),
            Times.Once);
    }
}
