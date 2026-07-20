import { type IndexingError } from "../../schemas";
import { getRelativePathFromPath } from "./getRelativePathFromPath";
import { getFileNameFromPath } from "./getFileNameFromPath";
import { ResolvePathFileType } from "./getGroupedResolvePaths";

const normalizePathSeparators = (path: string) => path.replace(/\\/g, "/");

const mapFromDestinationFullPath = (
  error: IndexingError,
  destinationFullPath: string,
): ResolvePathFileType => {
  const normalizedFullPath = normalizePathSeparators(destinationFullPath);
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
};

const mapFromDestinationAndSourcePath = (
  error: IndexingError,
  destinationPath: string,
): ResolvePathFileType => {
  const normalizedSourcePath = normalizePathSeparators(error.sourcePath);
  const relativeSourcePath = getRelativePathFromPath(normalizedSourcePath);

  return {
    id: error.id,
    relativeSourcePath,
    sourceName: getFileNameFromPath(normalizedSourcePath),
    relativeFinalPath: relativeSourcePath
      ? `${destinationPath}${relativeSourcePath}/`
      : destinationPath,
  };
};

export const getMappedResolvePathFiles = (
  errors: IndexingError[],
  destinationPath: string,
) => {
  const mappedFiles: ResolvePathFileType[] = errors.map((error) =>
    error.destinationFullPath
      ? mapFromDestinationFullPath(error, error.destinationFullPath)
      : mapFromDestinationAndSourcePath(error, destinationPath),
  );
  return mappedFiles;
};
