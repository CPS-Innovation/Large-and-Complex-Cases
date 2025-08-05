using AutoFixture;
using CPS.ComplexCases.FileTransfer.API.Durable.Activity;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using CPS.ComplexCases.FileTransfer.API.Models.Domain;
using CPS.ComplexCases.FileTransfer.API.Tests.Unit.Stubs;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Durable.Activity;

public class UpdateTransferStatusTests
{
    private readonly Fixture _fixture;
    private readonly UpdateTransferStatus _activity;

    public UpdateTransferStatusTests()
    {
        _fixture = new Fixture();
        _activity = new UpdateTransferStatus();
    }

    [Fact]
    public async Task Run_SignalsEntity_WithCorrectIdOperationAndStatus()
    {
        // Arrange
        var payload = _fixture.Create<UpdateTransferStatusPayload>();
        var entityClientStub = new DurableEntityClientStub("test");
        var clientStub = new DurableTaskClientStub(entityClientStub);

        // Act
        await _activity.Run(payload, clientStub, CancellationToken.None);

        // Assert
        Assert.True(entityClientStub.SignalEntityAsyncCalled);
        Assert.Equal(nameof(TransferEntityState).ToLowerInvariant(), entityClientStub.SignaledEntityId?.Name);
        Assert.Equal(payload.TransferId.ToString(), entityClientStub.SignaledEntityId?.Key);
        Assert.Equal(nameof(TransferEntityState.UpdateStatus), entityClientStub.SignaledOperationName);
    }
}
