import { IndexingError } from "../types/IndexingFileTransferResponse";
import { getRelativePathFromPath } from "./getRelativePathFromPath";
import { getFileNameFromPath } from "./getFileNameFromPath";

export type ResolvePathFileType = {
  id: string;
  relativePath: string;
  sourceName: string;
};

export const getGroupedResolvePaths = (errors: IndexingError[]) => {
  const errorFiles: ResolvePathFileType[] = errors.map((error) => ({
    id: error.id,
    relativePath: getRelativePathFromPath(error.sourcePath),
    sourceName: getFileNameFromPath(error.sourcePath),
  }));
  const groupedFiles = errorFiles.reduce(
    (acc, curr) => {
      if (!acc[`${curr.relativePath}`]) {
        acc[`${curr.relativePath}`] = [curr];
        return acc;
      }
      acc[`${curr.relativePath}`].push(curr);
      return acc;
    },
    {} as Record<string, ResolvePathFileType[]>,
  );

  return groupedFiles;
};
