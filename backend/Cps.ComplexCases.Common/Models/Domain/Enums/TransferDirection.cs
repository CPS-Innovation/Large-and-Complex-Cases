using System.Text.Json.Serialization;
using CPS.ComplexCases.Common.Attributes;

namespace CPS.ComplexCases.Common.Models.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TransferDirection
{
    [AlternateValue("Egress -> NetApp")]
    EgressToNetApp,
    [AlternateValue("NetApp -> Egress")]
    NetAppToEgress
}