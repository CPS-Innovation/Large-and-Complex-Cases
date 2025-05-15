import {
  NetAppFolderDataResponse,
  NetAppFolderData,
} from "../types/NetAppFolderData";

export const mapToNetAppFolderData = (
  responseData: NetAppFolderDataResponse,
): NetAppFolderData => {
  const folderData = responseData.folders.map((folder) => ({
    path: folder.path,
    filesize: 0,
    lastModified: "",
    isFolder: true,
  }));
  const fileData = responseData.files.map((file) => ({
    path: file.path,
    filesize: file.filesize,
    lastModified: file.lastModified,
    isFolder: false,
  }));

  return [...folderData, ...fileData];
};
