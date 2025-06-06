using System.Text.Json.Serialization;

namespace CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransferStatus
{
    Initiated, Queued, InProgress,
    InProgressChunking, UploadingChunk, CompletingMultipart, // Granular for items
    Completed, PartiallyCompleted, Failed, Cancelled, Pending // Pending for items
}
