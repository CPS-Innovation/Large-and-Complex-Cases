import { EgressFolderData } from "../types/EgressFolderData";
import { NetAppFolderData } from "../types/NetAppFolderData";
export const getGroupedFolderFileData = (
  selectedSourceFoldersOrFiles: string[],
  egressData: EgressFolderData | NetAppFolderData,
) => {
  const groupedData = selectedSourceFoldersOrFiles.reduce<{
    folders: string[];
    files: string[];
  }>(
    (acc, curr) => {
      const isFolder = egressData.find((data) => data.path === curr)?.isFolder;
      if (isFolder) acc.folders.push(curr);
      if (isFolder === false) acc.files.push(curr);
      return acc;
    },
    { folders: [], files: [] },
  );
  return groupedData;
};
