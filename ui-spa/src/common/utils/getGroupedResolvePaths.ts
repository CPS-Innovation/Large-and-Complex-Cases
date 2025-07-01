import { IndexingError } from "../types/IndexingFileTransferResponse";
import { getRelativePathFromPath } from "./getRelativePathFromPath";
import { getFileNameFromPath } from "./getFileNameFromPath";

export type ResolvePathFileType = {
  id: string;
  relativeSourcePath: string;
  sourceName: string;
  relativeFinalPath: string;
};

export const getMappedResolvePathFiles = (
  errors: IndexingError[],
  destinationPath: string,
) => {
  const mappedFiles: ResolvePathFileType[] = errors.map((error) => ({
    id: error.id,
    relativeSourcePath: getRelativePathFromPath(error.sourcePath),
    sourceName: getFileNameFromPath(error.sourcePath),
    relativeFinalPath: `${destinationPath}/${getRelativePathFromPath(error.sourcePath)}`,
  }));
  return mappedFiles;
};

export const getGroupedResolvePaths = (files: ResolvePathFileType[]) => {
  const groupedFiles = files.reduce(
    (acc, curr) => {
      if (!acc[`${curr.relativeSourcePath}`]) {
        acc[`${curr.relativeSourcePath}`] = [curr];
        return acc;
      }
      acc[`${curr.relativeSourcePath}`].push(curr);
      return acc;
    },
    {} as Record<string, ResolvePathFileType[]>,
  );

  return groupedFiles;
};
