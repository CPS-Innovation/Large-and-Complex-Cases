import { type EgressFolderData } from "../types/EgressFolderData";
import { type NetAppFolderData } from "../types/NetAppFolderData";
import { type TransferAction } from "../types/TransferAction";
import { getGroupedFolderFileData } from "./getGroupedFolderFileData";
export const getDuplicateFoldersAndFiles = (
  transferSource: "egress" | "netapp",
  transferAction: TransferAction,
  egressFolderData: EgressFolderData,
  netAppFolderData: NetAppFolderData,
  selectedSourceFoldersOrFiles: string[],
  currentEgressFolderPath: string,
  currentNetAppFolderPath: string,
) => {
  let shouldCheckForDuplicates = false;
  if (
    transferSource === "egress" &&
    transferAction.destinationFolder.path === currentNetAppFolderPath
  ) {
    shouldCheckForDuplicates = true;
  }
  if (
    transferSource === "netapp" &&
    transferAction.destinationFolder.path === currentEgressFolderPath
  ) {
    shouldCheckForDuplicates = true;
  }
  if (!shouldCheckForDuplicates) {
    return { folders: [], files: [] };
  }

  let sourceRootFolderPath = currentEgressFolderPath;
  let destinationRootFolderPath = currentNetAppFolderPath;

  if (transferSource === "netapp") {
    sourceRootFolderPath = currentNetAppFolderPath;
    destinationRootFolderPath = currentEgressFolderPath;
  }

  const destinationFolderData =
    transferSource === "egress" ? netAppFolderData : egressFolderData;

  const selectedSourceFolderAndFileNames = selectedSourceFoldersOrFiles.map(
    (item) => item.replace(sourceRootFolderPath, ""),
  );

  const destinationFolderAndFileNames = destinationFolderData.map((item) =>
    item.path.replace(destinationRootFolderPath, ""),
  );

  const duplicateItems = selectedSourceFolderAndFileNames.filter((name) =>
    destinationFolderAndFileNames.includes(name),
  );

  const duplicateFolderFilePaths = duplicateItems.map(
    (item) => `${sourceRootFolderPath}${item}`,
  );

  const groupedDuplicateFoldersAndFiles =
    transferSource === "egress"
      ? getGroupedFolderFileData(duplicateFolderFilePaths, egressFolderData)
      : getGroupedFolderFileData(duplicateFolderFilePaths, netAppFolderData);

  return groupedDuplicateFoldersAndFiles;
};
