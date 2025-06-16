namespace CPS.ComplexCases.Common.Extensions;

public static class PathExtensions
{
    public static string RemovePathPrefix(this string path, string? prefix)
    {
        if (prefix == null)
        {
            return path;
        }
        if (path.StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase))
        {
            return path[prefix.Length..].TrimStart(Path.DirectorySeparatorChar);
        }
        return path;
    }
}