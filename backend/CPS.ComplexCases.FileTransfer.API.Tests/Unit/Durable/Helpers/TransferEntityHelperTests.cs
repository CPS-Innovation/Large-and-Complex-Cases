using CPS.ComplexCases.FileTransfer.API.Durable.Helpers;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using CPS.ComplexCases.FileTransfer.API.Tests.Unit.Stubs;
using Microsoft.DurableTask.Client.Entities;
using Microsoft.DurableTask.Entities;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Helpers;

public class TransferEntityHelperTests
{
    private readonly DurableEntityClientStub _entityClientStub;
    private readonly DurableTaskClientStub _durableTaskClientStub;
    private readonly TransferEntityHelper _helper;

    public TransferEntityHelperTests()
    {
        _entityClientStub = new DurableEntityClientStub("TestEntityClient");
        _durableTaskClientStub = new DurableTaskClientStub(_entityClientStub);
        _helper = new TransferEntityHelper(_durableTaskClientStub);
    }

    [Fact]
    public async Task DeleteMovedItemsCompleted_SendsSignalWithCorrectParameters()
    {
        // Arrange
        var transferId = Guid.NewGuid();
        var failedItems = new List<DeletionError>
        {
            new DeletionError { FileId = "file1", ErrorMessage = "Error1" },
            new DeletionError { FileId = "file2", ErrorMessage = "Error2" }
        };
        var cts = new CancellationTokenSource();

        // Act
        await _helper.DeleteMovedItemsCompleted(transferId, failedItems, cts.Token);

        // Assert
        Assert.True(_entityClientStub.SignalEntityAsyncCalled);
        Assert.NotNull(_entityClientStub.SignaledEntityId);
        Assert.Equal(nameof(TransferEntityState.DeleteMovedItemsCompleted), _entityClientStub.SignaledOperationName);
    }


    [Fact]
    public async Task GetTransferEntityAsync_ReturnsEntity_WhenEntityExists()
    {
        // Arrange
        var transferId = Guid.NewGuid();
        var expectedEntityMetadata = new EntityMetadata<TransferEntity>(
            new EntityInstanceId(nameof(TransferEntityState), transferId.ToString()),
            new TransferEntity { DestinationPath = "some/path", BearerToken = "fakeBearerToken" });
        _entityClientStub.OnGetEntityAsync = (id, cancellation) =>
        {
            Assert.Equal(nameof(TransferEntityState), id.Name, ignoreCase: true);
            Assert.Equal(transferId.ToString(), id.Key);
            return Task.FromResult<EntityMetadata<TransferEntity>?>(expectedEntityMetadata);
        };

        // Act
        var result = await _helper.GetTransferEntityAsync(transferId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedEntityMetadata, result);
    }

    [Fact]
    public async Task GetTransferEntityAsync_ReturnsNull_WhenEntityDoesNotExist()
    {
        // Arrange
        var transferId = Guid.NewGuid();
        _entityClientStub.OnGetEntityAsync = (id, cancellation) => Task.FromResult<EntityMetadata<TransferEntity>?>(null);

        // Act
        var result = await _helper.GetTransferEntityAsync(transferId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetTransferEntityAsync_ThrowsIfDelegateNotSet()
    {
        // Arrange
        var transferId = Guid.NewGuid();
        _entityClientStub.OnGetEntityAsync = null!;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _helper.GetTransferEntityAsync(transferId));
    }
}
