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
  console.log("helloioooo i called you");
  const mappedFiles: ResolvePathFileType[] = errors.map((error) => ({
    id: error.id,
    relativeSourcePath: getRelativePathFromPath(error.sourcePath),
    sourceName: getFileNameFromPath(error.sourcePath),
    relativeFinalPath: `${destinationPath}/${getRelativePathFromPath(error.sourcePath)}`,
  }));
  return mappedFiles;
};

export const getGroupedResolvePaths = (
  files: ResolvePathFileType[],
  basePath: string,
) => {
  const groupedFiles = files.reduce(
    (acc, curr) => {
      const relativeSourcePath = curr.relativeSourcePath
        ? curr.relativeSourcePath
        : basePath;
      if (!acc[`${relativeSourcePath}`]) {
        acc[`${relativeSourcePath}`] = [curr];
        return acc;
      }
      acc[`${relativeSourcePath}`].push(curr);
      return acc;
    },
    {} as Record<string, ResolvePathFileType[]>,
  );

  return groupedFiles;
};
