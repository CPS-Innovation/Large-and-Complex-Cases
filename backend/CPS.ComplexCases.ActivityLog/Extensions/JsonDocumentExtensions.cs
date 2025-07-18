using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace CPS.ComplexCases.ActivityLog.Extensions;

public static class JsonDocumentExtensions
{
    public static T? DeserializeJsonDocument<T>(this JsonDocument document, ILogger? logger = null)
    {
        try
        {
            if (document == null)
            {
                logger?.LogWarning("Attempted to deserialize a null JsonDocument.");
                return default;
            }

            var json = document.RootElement.GetRawText();
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            logger?.LogError(ex, "Failed to deserialize JsonDocument to type {TypeName}.", typeof(T).Name);
            return default;
        }
    }

    public static JsonDocument SerializeToJsonDocument<T>(this T data, ILogger? logger = null)
    {
        try
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            return JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            logger?.LogError(ex, "Failed to serialize object to JsonDocument.");
            return JsonDocument.Parse("{}");
        }
    }
}