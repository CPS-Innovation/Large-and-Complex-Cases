using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;

public class TransferEntityStateTests
{
    [Fact]
    public void Initialize_SetsInitialState()
    {
        // Arrange
        var entity = new TransferEntity
        {
            Id = Guid.NewGuid(),
            Status = TransferStatus.Initiated,
            SuccessfulItems = new List<TransferItem>(),
            FailedItems = new List<TransferFailedItem>(),
            DeletionErrors = new List<DeletionError>(),
            SourcePaths = new List<TransferSourcePath>(),
            DestinationPath = "dest",
            BearerToken = "fakeBearerToken"
        };

        var state = new TransferEntityState();

        // Act
        state.Initialize(entity);

        // Assert
        Assert.Equal(entity, state.CurrentState);
    }

    [Fact]
    public void AddSuccessfulItem_UpdatesStateCorrectly()
    {
        // Arrange
        var state = new TransferEntityState();
        state.Initialize(new TransferEntity { DestinationPath = "dest", BearerToken = "fakeBearerToken" });

        var item = new TransferItem
        {
            SourcePath = "path1",
            Size = 1234,
            Status = TransferItemStatus.Completed,
            FileId = "file-1",
            IsRenamed = false
        };

        // Act
        state.AddSuccessfulItem(item);

        // Assert
        Assert.Single(state.CurrentState.SuccessfulItems);
        Assert.Equal(1, state.CurrentState.SuccessfulFiles);
        Assert.Equal(1, state.CurrentState.ProcessedFiles);
        Assert.Contains(item, state.CurrentState.SuccessfulItems);
    }

    [Fact]
    public void AddFailedItem_UpdatesStateCorrectly()
    {
        // Arrange
        var state = new TransferEntityState();
        state.Initialize(new TransferEntity { DestinationPath = "dest", BearerToken = "fakeBearerToken" });

        var item = new TransferFailedItem
        {
            SourcePath = "badfile",
            ErrorCode = TransferErrorCode.GeneralError,
            ErrorMessage = "Something failed"
        };

        // Act
        state.AddFailedItem(item);

        // Assert
        Assert.Single(state.CurrentState.FailedItems);
        Assert.Equal(1, state.CurrentState.FailedFiles);
        Assert.Equal(1, state.CurrentState.ProcessedFiles);
        Assert.Contains(item, state.CurrentState.FailedItems);
    }

    [Fact]
    public void FinalizeTransfer_WithOnlySuccessfulItems_SetsStatusToCompleted()
    {
        // Arrange
        var state = new TransferEntityState();
        state.Initialize(new TransferEntity
        {
            DestinationPath = "dest",
            BearerToken = "fakeBearerToken",
            SuccessfulItems = new List<TransferItem>
            {
                new TransferItem { SourcePath = "file1", Status = TransferItemStatus.Completed, IsRenamed = false, Size = 0 }
            },
            FailedItems = new List<TransferFailedItem>()
        });

        // Act
        state.FinalizeTransfer();

        // Assert
        Assert.Equal(TransferStatus.Completed, state.CurrentState.Status);
        Assert.NotNull(state.CurrentState.CompletedAt);
        Assert.True(state.CurrentState.CompletedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void FinalizeTransfer_WithFailedItems_SetsStatusToPartiallyCompleted()
    {
        // Arrange
        var state = new TransferEntityState();
        state.Initialize(new TransferEntity
        {
            DestinationPath = "dest",
            BearerToken = "fakeBearerToken",
            SuccessfulItems = new List<TransferItem>
            {
                new TransferItem { SourcePath = "file1", Status = TransferItemStatus.Completed, IsRenamed = false, Size = 0 }
            },
            FailedItems = new List<TransferFailedItem>
            {
                new TransferFailedItem { SourcePath = "file2", ErrorCode = TransferErrorCode.GeneralError }
            }
        });

        // Act
        state.FinalizeTransfer();

        // Assert
        Assert.Equal(TransferStatus.PartiallyCompleted, state.CurrentState.Status);
        Assert.NotNull(state.CurrentState.CompletedAt);
    }

    [Fact]
    public void DeleteMovedItemsCompleted_RecordsErrorsAndFlagsState()
    {
        // Arrange
        var state = new TransferEntityState();
        state.Initialize(new TransferEntity { DestinationPath = "dest", BearerToken = "fakeBearerToken" });

        var errors = new List<DeletionError>
        {
            new DeletionError { FileId = "file-1", ErrorMessage = "not found" }
        };

        // Act
        state.DeleteMovedItemsCompleted(errors);

        // Assert
        Assert.False(state.CurrentState.MovedFilesDeletedSuccessfully);
        Assert.Single(state.CurrentState.DeletionErrors);
    }

    [Fact]
    public void DeleteMovedItemsCompleted_WithNoErrors_SetsSuccessFlag()
    {
        // Arrange
        var state = new TransferEntityState();
        state.Initialize(new TransferEntity { DestinationPath = "dest", BearerToken = "fakeBearerToken" });

        var errors = new List<DeletionError>();

        // Act
        state.DeleteMovedItemsCompleted(errors);

        // Assert
        Assert.True(state.CurrentState.MovedFilesDeletedSuccessfully);
        Assert.Empty(state.CurrentState.DeletionErrors);
    }

    [Fact]
    public void RemoveTransientFailures_RemovesTransientItemsAndDecrementsCounters()
    {
        // Arrange
        var state = new TransferEntityState();
        state.Initialize(new TransferEntity { DestinationPath = "dest", BearerToken = "fakeBearerToken" });

        state.AddFailedItem(new TransferFailedItem { SourcePath = "file1", ErrorCode = TransferErrorCode.Transient, ErrorMessage = "S3 500" });
        state.AddFailedItem(new TransferFailedItem { SourcePath = "file2", ErrorCode = TransferErrorCode.Transient, ErrorMessage = "S3 404" });
        state.AddFailedItem(new TransferFailedItem { SourcePath = "file3", ErrorCode = TransferErrorCode.GeneralError, ErrorMessage = "other" });

        Assert.Equal(3, state.CurrentState.FailedFiles);
        Assert.Equal(3, state.CurrentState.ProcessedFiles);

        // Act
        state.RemoveTransientFailures();

        // Assert
        Assert.Single(state.CurrentState.FailedItems);
        Assert.Equal("file3", state.CurrentState.FailedItems[0].SourcePath);
        Assert.Equal(1, state.CurrentState.FailedFiles);
        Assert.Equal(3, state.CurrentState.ProcessedFiles); // ProcessedFiles stays at 3 -- never goes backwards
    }

    [Fact]
    public void RemoveTransientFailures_WhenNoTransientFailures_DoesNotModifyCounts()
    {
        // Arrange
        var state = new TransferEntityState();
        state.Initialize(new TransferEntity { DestinationPath = "dest", BearerToken = "fakeBearerToken" });

        state.AddFailedItem(new TransferFailedItem { SourcePath = "file1", ErrorCode = TransferErrorCode.GeneralError, ErrorMessage = "permanent" });
        state.AddSuccessfulItem(new TransferItem { SourcePath = "file2", Status = TransferItemStatus.Completed, IsRenamed = false, Size = 100 });

        // Act
        state.RemoveTransientFailures();

        // Assert
        Assert.Single(state.CurrentState.FailedItems);
        Assert.Single(state.CurrentState.SuccessfulItems);
        Assert.Equal(1, state.CurrentState.FailedFiles);
        Assert.Equal(1, state.CurrentState.SuccessfulFiles);
        Assert.Equal(2, state.CurrentState.ProcessedFiles);
    }

    [Fact]
    public void RemoveTransientFailures_WithOnlyTransient_RemovesAllAndZerosCounters()
    {
        // Arrange
        var state = new TransferEntityState();
        state.Initialize(new TransferEntity { DestinationPath = "dest", BearerToken = "fakeBearerToken" });

        state.AddFailedItem(new TransferFailedItem { SourcePath = "file1", ErrorCode = TransferErrorCode.Transient, ErrorMessage = "S3 500" });

        // Act
        state.RemoveTransientFailures();

        // Assert
        Assert.Empty(state.CurrentState.FailedItems);
        Assert.Equal(0, state.CurrentState.FailedFiles);
        Assert.Equal(1, state.CurrentState.ProcessedFiles); // ProcessedFiles stays at 1 -- never goes backwards
    }

    [Fact]
    public void AddSuccessfulRetryItem_DoesNotIncrementProcessedFiles()
    {
        // Arrange
        var state = new TransferEntityState();
        state.Initialize(new TransferEntity { DestinationPath = "dest", BearerToken = "fakeBearerToken" });

        // Simulate initial failure then retry
        state.AddFailedItem(new TransferFailedItem { SourcePath = "file1", ErrorCode = TransferErrorCode.Transient, ErrorMessage = "S3 500" });
        Assert.Equal(1, state.CurrentState.ProcessedFiles);

        state.RemoveTransientFailures();
        Assert.Equal(1, state.CurrentState.ProcessedFiles); // Still 1

        // Act -- retry succeeds
        state.AddSuccessfulRetryItem(new TransferItem { SourcePath = "file1", Status = TransferItemStatus.Completed, IsRenamed = false, Size = 500 });

        // Assert -- ProcessedFiles still 1, not 2
        Assert.Equal(1, state.CurrentState.ProcessedFiles);
        Assert.Equal(1, state.CurrentState.SuccessfulFiles);
        Assert.Equal(0, state.CurrentState.FailedFiles);
        Assert.Single(state.CurrentState.SuccessfulItems);
    }

    [Fact]
    public void AddFailedRetryItem_DoesNotIncrementProcessedFiles()
    {
        // Arrange
        var state = new TransferEntityState();
        state.Initialize(new TransferEntity { DestinationPath = "dest", BearerToken = "fakeBearerToken" });

        // Simulate initial transient failure then retry that fails permanently
        state.AddFailedItem(new TransferFailedItem { SourcePath = "file1", ErrorCode = TransferErrorCode.Transient, ErrorMessage = "S3 500" });
        state.RemoveTransientFailures();

        // Act -- retry fails permanently
        state.AddFailedRetryItem(new TransferFailedItem { SourcePath = "file1", ErrorCode = TransferErrorCode.GeneralError, ErrorMessage = "Persistent failure" });

        // Assert -- ProcessedFiles still 1, not 2
        Assert.Equal(1, state.CurrentState.ProcessedFiles);
        Assert.Equal(0, state.CurrentState.SuccessfulFiles);
        Assert.Equal(1, state.CurrentState.FailedFiles);
        Assert.Single(state.CurrentState.FailedItems);
        Assert.Equal(TransferErrorCode.GeneralError, state.CurrentState.FailedItems[0].ErrorCode);
    }
}
