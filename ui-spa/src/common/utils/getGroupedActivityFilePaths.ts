export type ActivityRelativePathFileType = {
  errors: { relativePathParts: string[]; fileName: string }[];
  success: { relativePathParts: string[]; fileName: string }[];
};

export const getGroupedActvityFilePaths = (
  successFiles: { sourcePath: string }[],
  failedFiles: { sourcePath: string }[],
  sourcePath: string,
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
  const sourcePathParts = sourcePath.split("/");
  const successFilePaths = successFiles.map(({ sourcePath: file }) => ({
    hasFailed: false,
    ...getRelativePathAndFileName(sourcePathParts.length, file),
  }));
  const failedFilePaths = failedFiles.map(({ sourcePath: file }) => ({
    hasFailed: true,
    ...getRelativePathAndFileName(sourcePathParts.length, file),
  }));

  const groupedFiles = [...failedFilePaths, ...successFilePaths].reduce(
    (acc, curr) => {
      if (!acc[`${curr.relativePath}`]) {
        const value: ActivityRelativePathFileType = {
          errors: [],
          success: [],
        };
        if (curr.hasFailed) {
          value.errors = [
            {
              relativePathParts: curr.relativePath.split(">"),
              fileName: curr.fileName,
            },
          ];
        } else {
          value.success = [
            {
              relativePathParts: curr.relativePath.split(">"),
              fileName: curr.fileName,
            },
          ];
        }

        acc[`${curr.relativePath}`] = value;
        return acc;
      }
      if (curr.hasFailed)
        acc[`${curr.relativePath}`].errors.push({
          relativePathParts: curr.relativePath.split(">"),
          fileName: curr.fileName,
        });

      acc[`${curr.relativePath}`].success.push({
        relativePathParts: curr.relativePath.split(">"),
        fileName: curr.fileName,
      });

      return acc;
    },
    {} as Record<string, ActivityRelativePathFileType>,
  );
  return groupedFiles;
};
