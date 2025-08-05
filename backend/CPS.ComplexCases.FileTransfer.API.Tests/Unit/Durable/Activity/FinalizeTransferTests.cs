using AutoFixture;
using CPS.ComplexCases.FileTransfer.API.Durable.Activity;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using CPS.ComplexCases.FileTransfer.API.Tests.Unit.Stubs;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Durable.Activity;

public class FinalizeTransferTests
{
    private readonly Fixture _fixture;
    private readonly FinalizeTransfer _activity;

    public FinalizeTransferTests()
    {
        _fixture = new Fixture();
        _activity = new FinalizeTransfer();
    }

    [Fact]
    public async Task Run_SignalsEntity_WithCorrectEntityIdAndOperation()
    {
        // Arrange
        var payload = _fixture.Create<FinalizeTransferPayload>();
        var entityClientStub = new DurableEntityClientStub("test");
        var durableClientStub = new DurableTaskClientStub(entityClientStub);

        // Act
        await _activity.Run(payload, durableClientStub, CancellationToken.None);

        // Assert
        Assert.True(entityClientStub.SignalEntityAsyncCalled);
        Assert.Equal(nameof(TransferEntityState).ToLowerInvariant(), entityClientStub.SignaledEntityId?.Name);
        Assert.Equal(payload.TransferId.ToString(), entityClientStub.SignaledEntityId?.Key);
        Assert.Equal(nameof(TransferEntityState.FinalizeTransfer), entityClientStub.SignaledOperationName);
    }
}
