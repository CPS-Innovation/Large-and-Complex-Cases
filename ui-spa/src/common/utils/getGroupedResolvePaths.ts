export type ResolvePathFileType = {
  id: string;
  relativeSourcePath: string;
  sourceName: string;
  relativeFinalPath: string;
};

export const getGroupedResolvePaths = (files: ResolvePathFileType[]) => {
  const groupedFiles = files.reduce(
    (acc, curr) => {
      const relativeFinalPath = curr.relativeFinalPath;

      if (!acc[`${relativeFinalPath}`]) {
        acc[`${relativeFinalPath}`] = [curr];
        return acc;
      }
      acc[`${relativeFinalPath}`].push(curr);
      return acc;
    },
    {} as Record<string, ResolvePathFileType[]>,
  );

  return groupedFiles;
};
