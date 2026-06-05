import type { NetAppFolderDataResponse, NetAppFolderData } from "../../schemas";
import { getFolderNameFromPath } from "../../common/utils/getFolderNameFromPath";
import { getFileNameFromPath } from "../../common/utils/getFileNameFromPath";
export const mapToNetAppFolderData = (
  responseData: NetAppFolderDataResponse,
): NetAppFolderData => {
  const folderData = responseData.folderData.map((folder) => ({
    path: folder.path,
    name: getFolderNameFromPath(folder.path),
    dateUpdated: "",
    filesize: 0,
    lastModified: "",
    isFolder: true,
  }));
  const fileData = responseData.fileData.map((file) => ({
    path: file.path,
    name: getFileNameFromPath(file.path),
    dateUpdated: file.lastModified,
    filesize: file.filesize,
    lastModified: file.lastModified,
    isFolder: false,
  }));

  return [...folderData, ...fileData];
};
