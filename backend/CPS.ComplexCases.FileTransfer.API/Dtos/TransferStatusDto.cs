using CPS.ComplexCases.Common.Models.Domain.Enums;
using CPS.ComplexCases.FileTransfer.API.Durable.Payloads.Domain;
using CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;

namespace CPS.ComplexCases.FileTransfer.API.Dtos;

public class TransferStatusDto
{
    public TransferStatus Status { get; init; }
    public TransferType TransferType { get; init; }
    public TransferDirection Direction { get; init; }
    public DateTime? CompletedAt { get; init; }
    public List<TransferFailedItemDto> FailedItems { get; init; } = [];
    public string? UserName { get; init; }
    public int TotalFiles { get; init; }
    public int ProcessedFiles { get; init; }

    public static TransferStatusDto From(TransferEntity entity) => new()
    {
        Status = entity.Status,
        TransferType = entity.TransferType,
        Direction = entity.Direction,
        CompletedAt = entity.CompletedAt,
        FailedItems = entity.FailedItems.Select(TransferFailedItemDto.From).ToList(),
        UserName = entity.UserName,
        TotalFiles = entity.TotalFiles,
        ProcessedFiles = entity.ProcessedFiles,
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
