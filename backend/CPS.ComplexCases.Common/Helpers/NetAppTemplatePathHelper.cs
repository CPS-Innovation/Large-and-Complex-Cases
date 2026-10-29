namespace CPS.ComplexCases.Common.Helpers;

public static class NetAppTemplatePathHelper
{
    public const string TemplatesFolderName = "_templates";

    public static string NormaliseParentFolderPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        var trimmed = path.Trim().TrimStart('/');
        if (trimmed.Length == 0)
            return string.Empty;

        return trimmed.EndsWith('/') ? trimmed : trimmed + "/";
    }

    public static bool IsWithinTemplatesFolder(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        return path
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Any(segment => string.Equals(segment, TemplatesFolderName, StringComparison.OrdinalIgnoreCase));
    }

    public static string GetTemplatesFolderPath(string? parentFolderPath) =>
        $"{NormaliseParentFolderPath(parentFolderPath)}{TemplatesFolderName}/";

    public static bool IsTemplateUnderParent(string? templatePath, string? parentFolderPath)
    {
        if (string.IsNullOrEmpty(templatePath))
            return false;

        var templatesFolder = GetTemplatesFolderPath(parentFolderPath);
        if (!templatePath.StartsWith(templatesFolder, StringComparison.OrdinalIgnoreCase))
            return false;

        return templatePath[templatesFolder.Length..].Trim('/').Length > 0;
    }

    public static string BuildDestinationPath(string? parentFolderPath, string caseName) =>
        $"{NormaliseParentFolderPath(parentFolderPath)}{caseName.Trim('/')}/";

    public static string GetParentFolderPath(string? destinationPath)
    {
        if (string.IsNullOrEmpty(destinationPath))
            return string.Empty;

        var trimmed = destinationPath.TrimEnd('/');
        var lastSlash = trimmed.LastIndexOf('/');
        return lastSlash >= 0 ? trimmed[..(lastSlash + 1)] : string.Empty;
    }

    public static string GetFolderName(string path)
    {
        var trimmed = path.TrimEnd('/');
        var lastSlash = trimmed.LastIndexOf('/');
        return lastSlash >= 0 ? trimmed[(lastSlash + 1)..] : trimmed;
    }
}
