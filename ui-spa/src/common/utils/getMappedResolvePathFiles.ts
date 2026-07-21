import { type IndexingError } from "../../schemas";
import { getRelativePathFromPath } from "./getRelativePathFromPath";
import { getFileNameFromPath } from "./getFileNameFromPath";
import { ResolvePathFileType } from "./getGroupedResolvePaths";

const normalizePathSeparators = (path: string) => path.replace(/\\/g, "/");

export const getMappedResolvePathFiles = (errors: IndexingError[]) => {
  const mappedFiles: ResolvePathFileType[] = errors.map((error) => {
    const normalizedFullPath = normalizePathSeparators(
      error.destinationFullPath,
    );
    const sourceName = getFileNameFromPath(normalizedFullPath);
    const relativeDir = getRelativePathFromPath(normalizedFullPath);

    return {
      id: error.id,
      relativeSourcePath: getRelativePathFromPath(
        normalizePathSeparators(error.sourcePath),
      ),
      sourceName,
      relativeFinalPath: relativeDir ? `${relativeDir}/` : "",
    };
  });
  return mappedFiles;
};
