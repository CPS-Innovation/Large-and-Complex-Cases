using CPS.ComplexCases.FileTransfer.API.Durable.Helpers;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using CPS.ComplexCases.FileTransfer.API.Telemetry;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CPS.ComplexCases.FileTransfer.API.Tests.Unit.Durable.Helpers;

public class TransferResultProcessorTests
{
    private readonly Mock<TaskOrchestrationContext> _context = new();
    private readonly EntityInstanceId _entityId = new(nameof(TransferEntityState), "transfer-1");

    public TransferResultProcessorTests()
    {
        _context.Setup(c => c.CreateReplaySafeLogger(It.IsAny<string>()))
            .Returns(NullLogger.Instance);
        _context.Setup(c => c.Entities.CallEntityAsync(
                It.IsAny<EntityInstanceId>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CallEntityOptions>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task ProcessAsync_MixedBatch_SendsOneEntityCallAndUpdatesTelemetry()
    {
        var telemetry = CreateTelemetry();
        var success = new TransferItem
        {
            SourcePath = "ok.txt",
            Status = TransferItemStatus.Completed,
            IsRenamed = false,
            Size = 40
        };
        var skipped = new TransferItem
        {
            SourcePath = "empty.txt",
            Status = TransferItemStatus.Skipped,
            IsRenamed = false,
            Size = 0
        };
        var failed = new TransferFailedItem
        {
            SourcePath = "bad.txt",
            ErrorCode = TransferErrorCode.GeneralError,
            ErrorMessage = "failed"
        };

        await TransferResultProcessor.ProcessAsync(
            _context.Object,
            _entityId,
            [
                new TransferResult { IsSuccess = true, SuccessfulItem = success },
                null!,
                new TransferResult { IsSkipped = true, SkippedItem = skipped },
                new TransferResult { IsSuccess = false, FailedItem = failed }
            ],
            telemetry);

        Assert.Equal(1, telemetry.TotalFilesTransferred);
        Assert.Equal(40, telemetry.TotalBytesTransferred);
        Assert.Equal(1, telemetry.TotalFilesFailed);

        _context.Verify(c => c.Entities.CallEntityAsync(
                _entityId,
                nameof(TransferEntityState.ApplyResultBatch),
                It.Is<TransferResultBatch>(batch =>
                    !batch.IsRetry
                    && batch.SuccessfulItems.Count == 1
                    && batch.SuccessfulItems[0].SourcePath == "ok.txt"
                    && batch.SkippedItems.Count == 1
                    && batch.FailedItems.Count == 1
                    && batch.FailedItems[0].SourcePath == "bad.txt"),
                It.IsAny<CallEntityOptions>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_RetryBatch_FlagsTheBatchAsRetry()
    {
        var telemetry = CreateTelemetry();

        await TransferResultProcessor.ProcessAsync(
            _context.Object,
            _entityId,
            [
                new TransferResult
                {
                    IsSuccess = true,
                    SuccessfulItem = new TransferItem
                    {
                        SourcePath = "retried.txt",
                        Status = TransferItemStatus.Completed,
                        IsRenamed = false,
                        Size = 8
                    }
                }
            ],
            telemetry,
            isRetry: true);

        _context.Verify(c => c.Entities.CallEntityAsync(
                _entityId,
                nameof(TransferEntityState.ApplyResultBatch),
                It.Is<TransferResultBatch>(batch => batch.IsRetry && batch.SuccessfulItems.Count == 1),
                It.IsAny<CallEntityOptions>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_OnlyNullResults_DoesNotCallTheEntity()
    {
        await TransferResultProcessor.ProcessAsync(
            _context.Object,
            _entityId,
            [null!, null!],
            CreateTelemetry());

        _context.Verify(c => c.Entities.CallEntityAsync(
                It.IsAny<EntityInstanceId>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CallEntityOptions>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessAsync_UnclassifiableResult_RecordsAFailureInTheBatch()
    {
        var telemetry = CreateTelemetry();

        await TransferResultProcessor.ProcessAsync(
            _context.Object,
            _entityId,
            [new TransferResult { IsSuccess = false }],
            telemetry);

        Assert.Equal(1, telemetry.TotalFilesFailed);
        _context.Verify(c => c.Entities.CallEntityAsync(
                _entityId,
                nameof(TransferEntityState.ApplyResultBatch),
                It.Is<TransferResultBatch>(batch =>
                    batch.FailedItems.Count == 1
                    && batch.FailedItems[0].SourcePath == "unknown"
                    && batch.FailedItems[0].ErrorCode == TransferErrorCode.GeneralError),
                It.IsAny<CallEntityOptions>()),
            Times.Once);
    }

    private static TransferOrchestrationEvent CreateTelemetry() => new()
    {
        TransferDirection = "NetAppToEgress",
        BucketName = "bucket"
    };
}
