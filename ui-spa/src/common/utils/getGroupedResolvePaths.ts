export type ResolvePathFileType = {
  id: string;
  relativeSourcePath: string;
  sourceName: string;
  relativeFinalPath: string;
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
