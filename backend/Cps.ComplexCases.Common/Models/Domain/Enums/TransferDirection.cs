using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Common.Models.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransferDirection
{
    EgressToNetApp,
    NetAppToEgress
}