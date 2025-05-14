import { NetAppFolderResponse } from "../../common/types/NetAppFolderData";
export const netAppRootFolderResultsDev: NetAppFolderResponse = {
  data: {
    files: [
      {
        filePath: "file-0-0",
        lastModified: "2000-01-02",
        size: 1234,
      },
      {
        filePath: "file-1-0",
        lastModified: "2000-01-03",
        size: 226897,
      },
    ],
    folders: [
      {
        folderPath: "folder-0-0/",
      },
      {
        folderPath: "folder-0-1/",
      },

      {
        folderPath: "folder-0-2/",
      },
    ],
  },

  pagination: {
    maxKeys: 100,
    nextContinuationToken: null,
  },
};

export const getNetAppFolderResultsDev = (path: string) => {
  if (!path || path === "abc") return netAppRootFolderResultsDev;

  const levels = path.split("/").filter((part) => part.length > 0);
  if (levels.length > 3) {
    return {
      ...netAppRootFolderResultsDev,
      data: {
        folders: [],
        files: [],
      },
    };
  }
  const newFolders = netAppRootFolderResultsDev.data.folders.map(
    (item, index) => {
      return {
        ...item,
        folderPath: `${path}folder-${levels.length}-${index}/`,
      };
    },
  );
  const newFiles = netAppRootFolderResultsDev.data.files.map((item, index) => {
    return { ...item, filePath: `files-${levels.length}-${index}/` };
  });

  return {
    ...netAppRootFolderResultsDev,
    data: {
      folders: newFolders,
      files: newFiles,
    },
  };
};
