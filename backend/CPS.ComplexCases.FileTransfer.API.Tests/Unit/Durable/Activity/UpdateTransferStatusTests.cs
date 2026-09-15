using AutoFixture;
using CPS.ComplexCases.FileTransfer.API.Durable.Activity;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using CPS.ComplexCases.FileTransfer.API.Tests.Unit.Stubs;
using Microsoft.Extensions.Logging.Abstractions;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Durable.Activity;

public class UpdateTransferStatusTests
{
    private readonly Fixture _fixture;
    private readonly UpdateTransferStatus _activity;

    public UpdateTransferStatusTests()
    {
        _fixture = new Fixture();
        _activity = new UpdateTransferStatus(NullLogger<UpdateTransferStatus>.Instance);
    }

    [Fact]
    public async Task Run_SignalsEntity_WithCorrectIdOperationAndStatus()
    {
        // Arrange
        var payload = _fixture.Build<UpdateTransferStatusPayload>()
            .Without(p => p.ErrorMessage)
            .Create();
        var entityClientStub = new DurableEntityClientStub("test");
        var clientStub = new DurableTaskClientStub(entityClientStub);

        // Act
        await _activity.Run(payload, clientStub, CancellationToken.None);

        // Assert
        Assert.True(entityClientStub.SignalEntityAsyncCalled);
        Assert.Equal(nameof(TransferEntityState).ToLowerInvariant(), entityClientStub.SignaledEntityId?.Name);
        Assert.Equal(payload.TransferId.ToString(), entityClientStub.SignaledEntityId?.Key);
        Assert.Equal(nameof(TransferEntityState.UpdateStatus), entityClientStub.SignaledOperationName);
        Assert.Single(entityClientStub.SignalledCalls);
        Assert.Same(payload, entityClientStub.SignalledCalls[0].Input);
    }

    [Fact]
    public async Task Run_WhenErrorMessageIsSet_SignalsUpdateStatusOnceWithPayload()
    {
        var payload = _fixture.Build<UpdateTransferStatusPayload>()
            .With(p => p.Status, TransferStatus.Failed)
            .With(p => p.ErrorMessage, "The transfer failed before any files were processed. Entity not found after retries.")
            .Create();
        var entityClientStub = new DurableEntityClientStub("test");
        var clientStub = new DurableTaskClientStub(entityClientStub);

        await _activity.Run(payload, clientStub, CancellationToken.None);

        Assert.Single(entityClientStub.SignalledCalls);
        Assert.Equal(nameof(TransferEntityState.UpdateStatus), entityClientStub.SignalledCalls[0].Operation);
        Assert.Same(payload, entityClientStub.SignalledCalls[0].Input);
    }
}
