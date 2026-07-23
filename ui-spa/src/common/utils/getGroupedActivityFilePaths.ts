import { getCleanPath } from "./getCleanPath";

export type ActivityRelativePathFileType = {
  errors: { fileName: string }[];
  skipped: { fileName: string }[];
  success: { fileName: string }[];
};

type GroupedFilePath = {
  relativePath: string;
  fileName: string;
  outcome: "failed" | "skipped" | "success";
};

export const getGroupedActivityFilePaths = (
  successFiles: { path: string }[],
  failedFiles: { path: string }[],
  sourcePath: string,
  skippedFiles: { path: string }[] = [],
) => {
  const getRelativePathAndFileName = (
    sourcePartsLength: number,
    filePath: string,
  ) => {
    const pathParts = filePath.split("/");
    return {
      relativePath: pathParts.slice(sourcePartsLength, -1).join(" > "),
      fileName: pathParts[pathParts.length - 1],
    };
  };

  const cleanSourcePath = getCleanPath(sourcePath);
  const sourcePathParts = cleanSourcePath ? cleanSourcePath.split("/") : [];

  const successFilePaths: GroupedFilePath[] = successFiles.map(({ path }) => ({
    outcome: "success",
    ...getRelativePathAndFileName(sourcePathParts.length, path),
  }));

  const failedFilePaths: GroupedFilePath[] = failedFiles.map(({ path }) => ({
    outcome: "failed",
    ...getRelativePathAndFileName(sourcePathParts.length, path),
  }));

  const skippedFilePaths: GroupedFilePath[] = skippedFiles.map(({ path }) => ({
    outcome: "skipped",
    ...getRelativePathAndFileName(sourcePathParts.length, path),
  }));

  const groupedFiles = [
    ...failedFilePaths,
    ...skippedFilePaths,
    ...successFilePaths,
  ].reduce(
    (acc, curr) => {
      if (!acc[curr.relativePath]) {
        acc[curr.relativePath] = {
          errors: [],
          skipped: [],
          success: [],
        };
      }

      if (curr.outcome === "failed") {
        acc[curr.relativePath].errors.push({ fileName: curr.fileName });
      } else if (curr.outcome === "skipped") {
        acc[curr.relativePath].skipped.push({ fileName: curr.fileName });
      } else {
        acc[curr.relativePath].success.push({ fileName: curr.fileName });
      }

      return acc;
    },
    {} as Record<string, ActivityRelativePathFileType>,
  );

  return groupedFiles;
};
