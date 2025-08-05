using AutoFixture;
using CPS.ComplexCases.FileTransfer.API.Durable.Activity;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using CPS.ComplexCases.FileTransfer.API.Tests.Unit.Stubs;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Durable.Activity;

public class InitializeTransferTests
{
    private readonly Fixture _fixture;
    private readonly InitializeTransfer _activity;

    public InitializeTransferTests()
    {
        _fixture = new Fixture();
        _activity = new InitializeTransfer();
    }

    [Fact]
    public async Task Run_SignalsEntity_WithCorrectEntityIdOperationAndPayload()
    {
        // Arrange
        var initialEntity = _fixture.Create<TransferEntity>();
        var entityClientStub = new DurableEntityClientStub("test");
        var durableClientStub = new DurableTaskClientStub(entityClientStub);

        // Act
        await _activity.Run(initialEntity, durableClientStub, CancellationToken.None);

        // Assert
        Assert.True(entityClientStub.SignalEntityAsyncCalled);
        Assert.Equal(nameof(TransferEntityState).ToLowerInvariant(), entityClientStub.SignaledEntityId?.Name);
        Assert.Equal(initialEntity.Id.ToString(), entityClientStub.SignaledEntityId?.Key);
        Assert.Equal(nameof(TransferEntityState.Initialize), entityClientStub.SignaledOperationName);
    }
}
