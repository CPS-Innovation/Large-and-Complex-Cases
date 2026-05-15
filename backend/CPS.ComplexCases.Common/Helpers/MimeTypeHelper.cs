using MimeTypes;

namespace CPS.ComplexCases.Common.Helpers;

public static class MimeTypeHelper
{
    private const string DefaultContentType = "application/octet-stream";

    public static string? GetMimeType(string? filePath)
    {
        if (filePath == null)
            return null;

        return MimeTypeMap.TryGetMimeType(filePath, out var contentType)
            ? contentType
            : DefaultContentType;
    }

    public static string? GetExtensionFromMimeType(string? mimeType)
    {
        if (mimeType == null)
            return null;

        try
        {
            return MimeTypeMap.GetExtension(mimeType);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}
