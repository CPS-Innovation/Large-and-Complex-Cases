using System;
using System.Collections.Generic;
using CPS.ComplexCases.Common.Models.Requests;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Durable.State;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;
using Xunit;

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
        state.Initialize(new TransferEntity { DestinationPath = "dest" });

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
        state.Initialize(new TransferEntity { DestinationPath = "dest" });

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
        state.Initialize(new TransferEntity { DestinationPath = "dest" });

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
        state.Initialize(new TransferEntity { DestinationPath = "dest" });

        var errors = new List<DeletionError>();

        // Act
        state.DeleteMovedItemsCompleted(errors);

        // Assert
        Assert.True(state.CurrentState.MovedFilesDeletedSuccessfully);
        Assert.Empty(state.CurrentState.DeletionErrors);
    }
}
