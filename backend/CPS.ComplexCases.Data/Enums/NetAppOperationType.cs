using System.Text.Json.Serialization;

namespace CPS.ComplexCases.Data.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NetAppOperationType
{
    Material,
    Folder
}