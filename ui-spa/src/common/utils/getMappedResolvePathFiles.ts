import { IndexingError } from "../types/IndexingFileTransferResponse";
import { getRelativePathFromPath } from "./getRelativePathFromPath";
import { getFileNameFromPath } from "./getFileNameFromPath";
import { ResolvePathFileType } from "./getGroupedResolvePaths";

export const getMappedResolvePathFiles = (
  errors: IndexingError[],
  destinationPath: string,
) => {
  const mappedFiles: ResolvePathFileType[] = errors.map((error) => ({
    id: error.id,
    relativeSourcePath: getRelativePathFromPath(error.sourcePath),
    sourceName: getFileNameFromPath(error.sourcePath),
    relativeFinalPath: getRelativePathFromPath(error.sourcePath)
      ? `${destinationPath}/${getRelativePathFromPath(error.sourcePath)}`
      : destinationPath,
  }));
  return mappedFiles;
};
