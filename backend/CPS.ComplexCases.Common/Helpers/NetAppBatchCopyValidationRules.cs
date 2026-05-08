namespace CPS.ComplexCases.Common.Helpers;

public static class NetAppBatchCopyValidationRules
{
    public const int MaxOperations = 100;

    public static bool ContainsTraversal(string path) =>
        path.Contains("..");

    public static bool StartsWithSlash(string path) =>
        path.StartsWith('/');

    public static bool IsFolderType(string type) =>
        string.Equals(type, "Folder", StringComparison.OrdinalIgnoreCase);

    public static bool IsMaterialType(string type) =>
        string.Equals(type, "Material", StringComparison.OrdinalIgnoreCase);

    public static bool HasDuplicateSourcePaths(IList<string> sourcePaths) =>
        sourcePaths.Distinct(StringComparer.OrdinalIgnoreCase).Count() != sourcePaths.Count;

    public static bool HasOverlappingPaths(IList<string> sourcePaths)
    {
        for (var i = 0; i < sourcePaths.Count; i++)
            for (var j = i + 1; j < sourcePaths.Count; j++)
                if (PathsOverlap(sourcePaths[i], sourcePaths[j]))
                    return true;
        return false;
    }


    public static IEnumerable<string> GetCrossFieldErrors(
        IEnumerable<(string Type, string SourcePath)> operations,
        string destinationPrefix)
    {
        foreach (var op in operations)
        {
            if (string.IsNullOrEmpty(op.SourcePath))
                continue;

            if (IsMaterialType(op.Type))
            {
                var fileName = Path.GetFileName(op.SourcePath);
                var computedDest = destinationPrefix + fileName;
                if (string.Equals(op.SourcePath, computedDest, StringComparison.OrdinalIgnoreCase))
                    yield return $"Source and destination are the same for path '{op.SourcePath}'.";
            }

            if (IsFolderType(op.Type))
            {
                var sourcePrefix = op.SourcePath.EndsWith('/') ? op.SourcePath : op.SourcePath + "/";
                if (destinationPrefix.StartsWith(sourcePrefix, StringComparison.OrdinalIgnoreCase))
                    yield return $"Folder copy destination '{destinationPrefix}' is a child of source '{op.SourcePath}'. Cannot copy a folder into itself.";
            }
        }
    }

    public static bool PathsOverlap(string pathA, string pathB)
    {
        var a = pathA.TrimEnd('/');
        var b = pathB.TrimEnd('/');

        return string.Equals(a, b, StringComparison.OrdinalIgnoreCase)
            || a.StartsWith(b + "/", StringComparison.OrdinalIgnoreCase)
            || b.StartsWith(a + "/", StringComparison.OrdinalIgnoreCase);
    }
}
