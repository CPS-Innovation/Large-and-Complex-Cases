using System.Text.Json.Serialization;

namespace CPS.ComplexCases.FileTransfer.API.Models.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransferStatus
{
    Initiated, InProgress,
    Completed, PartiallyCompleted, Failed
}
