using Microsoft.Extensions.Logging;
using CPS.ComplexCases.Common.Handlers;
using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.Common.Storage;
using CPS.ComplexCases.Common.Telemetry;
using CPS.ComplexCases.FileTransfer.API.Durable.Activity;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Factories;
using Moq;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Durable.Activity;

public class CreateDestinationFolderTests
{
    private readonly Mock<ILogger<CreateDestinationFolder>> _loggerMock;
    private readonly Mock<IStorageClientFactory> _storageClientFactoryMock;
    private readonly Mock<IStorageClient> _storageClientMock;
    private readonly Mock<IInitializationHandler> _initializationHandlerMock;
    private readonly Mock<ITelemetryClient> _telemetryClientMock;
    private readonly CreateDestinationFolder _activity;

    private const string BearerToken = "test-bearer";
    private const string BucketName = "test-bucket";
    private const string UserName = "testuser";
    private const string DestinationFolderPath = "CaseRoot/Dest/SubFolder/";

    public CreateDestinationFolderTests()
    {
        _loggerMock = new Mock<ILogger<CreateDestinationFolder>>();
        _storageClientFactoryMock = new Mock<IStorageClientFactory>();
        _storageClientMock = new Mock<IStorageClient>();
        _initializationHandlerMock = new Mock<IInitializationHandler>();
        _telemetryClientMock = new Mock<ITelemetryClient>();

        _storageClientFactoryMock
            .Setup(f => f.GetSourceClientForDirection(It.IsAny<TransferDirection>()))
            .Returns(_storageClientMock.Object);

        _storageClientMock
            .Setup(c => c.CreateFolderAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(true);

        _activity = new CreateDestinationFolder(
            _loggerMock.Object,
            _storageClientFactoryMock.Object,
            _initializationHandlerMock.Object,
            _telemetryClientMock.Object);
    }

    private static CreateDestinationFolderPayload CreateValidPayload() => new()
    {
        CaseId = 42,
        UserName = UserName,
        CorrelationId = Guid.NewGuid(),
        BearerToken = BearerToken,
        BucketName = BucketName,
        DestinationFolderPath = DestinationFolderPath,
        TransferDirection = TransferDirection.NetAppToNetApp,
    };

    [Fact]
    public async Task Run_WithValidPayload_CallsCreateFolderAsyncWithCorrectArguments()
    {
        // Arrange
        var payload = CreateValidPayload();

        // Act
        await _activity.Run(payload);

        // Assert
        _storageClientMock.Verify(c => c.CreateFolderAsync(
            payload.DestinationFolderPath,
            null,
            payload.BearerToken,
            payload.BucketName), Times.Once);
    }

    [Fact]
    public async Task Run_WithValidPayload_GetsSourceClientForCorrectDirection()
    {
        // Arrange
        var payload = CreateValidPayload();

        // Act
        await _activity.Run(payload);

        // Assert
        _storageClientFactoryMock.Verify(f => f.GetSourceClientForDirection(payload.TransferDirection), Times.Once);
    }

    [Fact]
    public async Task Run_WithNullPayload_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _activity.Run(null));
    }

    [Fact]
    public async Task Run_WithNullBucketName_ThrowsArgumentException()
    {
        // Arrange
        var payload = new CreateDestinationFolderPayload
        {
            CaseId = 42,
            UserName = UserName,
            BearerToken = BearerToken,
            BucketName = null!,
            DestinationFolderPath = DestinationFolderPath,
            TransferDirection = TransferDirection.NetAppToNetApp,
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _activity.Run(payload));
    }

    [Fact]
    public async Task Run_WithEmptyBucketName_ThrowsArgumentException()
    {
        // Arrange
        var payload = new CreateDestinationFolderPayload
        {
            CaseId = 42,
            UserName = UserName,
            BearerToken = BearerToken,
            BucketName = string.Empty,
            DestinationFolderPath = DestinationFolderPath,
            TransferDirection = TransferDirection.NetAppToNetApp,
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _activity.Run(payload));
    }

    [Fact]
    public async Task Run_WithValidPayload_InitializesHandlerWithPayloadContext()
    {
        // Arrange
        var payload = CreateValidPayload();

        // Act
        await _activity.Run(payload);

        // Assert
        _initializationHandlerMock.Verify(h => h.Initialize(
            payload.UserName!,
            payload.CorrelationId,
            payload.CaseId), Times.Once);
    }

    [Fact]
    public async Task Run_WhenCreateFolderReturnsFalse_ThrowsInvalidOperationException()
    {
        var payload = CreateValidPayload();

        _storageClientMock
            .Setup(c => c.CreateFolderAsync(
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _activity.Run(payload));
    }

    [Fact]
    public async Task Run_WhenCreateFolderThrows_ExceptionIsPropagated()
    {
        // Arrange
        var payload = CreateValidPayload();

        _storageClientMock
            .Setup(c => c.CreateFolderAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ThrowsAsync(new InvalidOperationException("Storage error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _activity.Run(payload));
    }

    [Fact]
    public async Task Run_WhenCreateFolderThrows_DoesNotSwallowException()
    {
        // Arrange
        var payload = CreateValidPayload();
        var expectedException = new HttpRequestException("Connection refused");

        _storageClientMock
            .Setup(c => c.CreateFolderAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .ThrowsAsync(expectedException);

        // Act
        var ex = await Record.ExceptionAsync(() => _activity.Run(payload));

        // Assert
        Assert.Same(expectedException, ex);
    }
}
