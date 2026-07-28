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
        var operationList = operations.ToList();

        foreach (var op in operationList)
        {
            if (string.IsNullOrEmpty(op.SourcePath))
                continue;

            if (IsMaterialType(op.Type))
            {
                var computedDest = ComputeDestinationPath(op.Type, op.SourcePath, destinationPrefix);
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

        foreach (var error in GetDuplicateDestinationErrors(operationList, destinationPrefix))
            yield return error;
    }

    public static IEnumerable<string> GetDuplicateDestinationErrors(
        IEnumerable<(string Type, string SourcePath)> operations,
        string destinationPrefix)
    {
        var destinations = operations
            .Where(op => !string.IsNullOrEmpty(op.SourcePath))
            .Select(op => ComputeDestinationPath(op.Type, op.SourcePath, destinationPrefix))
            .Where(dest => !string.IsNullOrEmpty(dest))
            .ToList();

        return destinations
            .GroupBy(dest => dest, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => $"Duplicate destination within batch: {g.Key}");
    }

    public static string ComputeDestinationPath(string type, string sourcePath, string destinationPrefix)
    {
        var trimmed = sourcePath.TrimEnd('/');
        var lastSlash = trimmed.LastIndexOf('/');
        var name = lastSlash >= 0 ? trimmed[(lastSlash + 1)..] : trimmed;

        if (IsFolderType(type))
            return $"{destinationPrefix}{name}/";

        return $"{destinationPrefix}{name}";
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
