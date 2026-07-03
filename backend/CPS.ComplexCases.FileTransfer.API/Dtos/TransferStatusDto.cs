using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;

namespace CPS.ComplexCases.FileTransfer.API.Dtos;

public class TransferStatusDto
{
    public Guid Id { get; init; }
    public TransferStatus Status { get; init; }
    public TransferType TransferType { get; init; }
    public TransferDirection Direction { get; init; }
    public required string DestinationPath { get; init; }
    public string? SourceRootFolderPath { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public List<TransferFailedItemDto> FailedItems { get; init; } = [];
    public List<TransferSuccessfulItemDto> SuccessfulItems { get; init; } = [];
    public string? UserName { get; init; }
    public int TotalFiles { get; init; }
    public int ProcessedFiles { get; init; }
    public int SuccessfulFiles { get; init; }
    public int FailedFiles { get; init; }

    public static TransferStatusDto From(TransferEntity entity) => new()
    {
        Id = entity.Id,
        Status = entity.Status,
        TransferType = entity.TransferType,
        Direction = entity.Direction,
        DestinationPath = entity.DestinationPath,
        SourceRootFolderPath = entity.SourceRootFolderPath,
        StartedAt = entity.StartedAt,
        CompletedAt = entity.CompletedAt,
        FailedItems = entity.FailedItems.Select(TransferFailedItemDto.From).ToList(),
        SuccessfulItems = entity.Status is TransferStatus.Completed
                or TransferStatus.PartiallyCompleted or TransferStatus.Failed
            ? entity.SuccessfulItems.Select(TransferSuccessfulItemDto.From).ToList()
            : [],
        UserName = entity.UserName,
        TotalFiles = entity.TotalFiles,
        ProcessedFiles = entity.ProcessedFiles,
        SuccessfulFiles = entity.SuccessfulFiles,
        FailedFiles = entity.FailedFiles,
    };
}

public class TransferFailedItemDto
{
    public required string SourcePath { get; init; }
    public TransferErrorCode ErrorCode { get; init; }

    public static TransferFailedItemDto From(TransferFailedItem item) => new()
    {
        SourcePath = item.SourcePath,
        ErrorCode = item.ErrorCode,
    };
}

public class TransferSuccessfulItemDto
{
    public required string SourcePath { get; init; }
    public long Size { get; init; }

    public static TransferSuccessfulItemDto From(TransferItem item) => new()
    {
        SourcePath = item.SourcePath,
        Size = item.Size,
    };
}
