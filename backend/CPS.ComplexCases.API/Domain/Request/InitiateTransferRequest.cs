using System.Text.Json.Serialization;
using CPS.ComplexCases.Common.Models.Domain.Enums;

namespace CPS.ComplexCases.API.Domain.Request;

public class InitiateTransferRequest
{
    [JsonPropertyName("transferType")]
    public TransferType TransferType { get; set; }
    [JsonPropertyName("sourcePaths")]
    public required List<string> SourcePaths { get; set; }
    [JsonPropertyName("destinationPath")]
    public required string DestinationPath { get; set; }
}