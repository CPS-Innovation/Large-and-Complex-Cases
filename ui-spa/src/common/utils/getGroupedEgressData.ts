import { EgressFolderData } from "../types/EgressFolderData";

export const getGroupedEgressData = (
  selectedSourceFoldersOrFiles: string[],
  egressData: EgressFolderData,
) => {
  const groupedData = selectedSourceFoldersOrFiles.reduce<{
    folders: string[];
    files: string[];
  }>(
    (acc, curr) => {
      const isFolder = egressData.find((data) => data.id === curr)?.isFolder;
      if (isFolder) acc.folders.push(curr);
      if (isFolder === false) acc.files.push(curr);
      return acc;
    },
    { folders: [], files: [] },
  );
  return groupedData;
};
