using System.Text.Json;

namespace CPS.ComplexCases.Common.Extensions;

public static class JsonSerializerExtensions
{
    public static string SerializeWithCamelCase<T>(this T data)
    {
        return JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
    public static T DeserializeFromCamelCase<T>(this string json)
    {
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        })!;
    }
}