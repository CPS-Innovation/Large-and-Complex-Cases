using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Storage;
using CPS.ComplexCases.FileTransfer.API.Durable.Activity;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Factories;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using Microsoft.Extensions.Logging;
using Moq;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Durable.Activity;

public class CreateEgressDestinationFoldersTests
{
    private readonly Mock<ILogger<CreateEgressDestinationFolders>> _loggerMock = new();
    private readonly Mock<IStorageClientFactory> _storageClientFactoryMock = new();
    private readonly Mock<IStorageClient> _egressClientMock = new();
    private readonly Mock<IInitializationHandler> _initializationHandlerMock = new();
    private readonly CreateEgressDestinationFolders _activity;

    public CreateEgressDestinationFoldersTests()
    {
        _storageClientFactoryMock
            .Setup(f => f.GetClient(StorageProvider.Egress))
            .Returns(_egressClientMock.Object);

        _egressClientMock
            .Setup(c => c.CreateFolderAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(true);

        _activity = new CreateEgressDestinationFolders(
            _loggerMock.Object,
            _storageClientFactoryMock.Object,
            _initializationHandlerMock.Object);
    }

    private static CreateEgressFoldersPayload CreatePayload(List<string> folderPaths) => new()
    {
        WorkspaceId = "ws-1",
        FolderPaths = folderPaths,
        CaseId = 42,
        UserName = "testuser",
        CorrelationId = Guid.NewGuid()
    };

    [Fact]
    public async Task Run_CreatesEachFolderOnceWithWorkspaceId()
    {
        var payload = CreatePayload(new List<string> { "dest/A", "dest/B" });

        await _activity.Run(payload);

        _egressClientMock.Verify(c => c.CreateFolderAsync("dest/A", payload.WorkspaceId, null, null), Times.Once);
        _egressClientMock.Verify(c => c.CreateFolderAsync("dest/B", payload.WorkspaceId, null, null), Times.Once);
    }

    [Fact]
    public async Task Run_WithNoFolders_DoesNotCallCreateFolder()
    {
        var payload = CreatePayload(new List<string>());

        await _activity.Run(payload);

        _egressClientMock.Verify(
            c => c.CreateFolderAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    public async Task Run_InitializesHandlerWithPayloadContext()
    {
        var payload = CreatePayload(new List<string> { "dest/A" });

        await _activity.Run(payload);

        _initializationHandlerMock.Verify(h => h.Initialize(
            payload.UserName!,
            payload.CorrelationId,
            payload.CaseId), Times.Once);
    }
}
