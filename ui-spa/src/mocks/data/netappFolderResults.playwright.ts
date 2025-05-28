import { NetAppFolderResponse } from "../../common/types/NetAppFolderData";
export const netAppRootFolderResultsPlaywright: NetAppFolderResponse = {
  data: {
    fileData: [
      {
        path: "netapp/file-1-0.pdf",
        lastModified: "2000-01-02",
        filesize: 1234,
      },
      {
        path: "netapp/file-1-1.pdf",
        lastModified: "2000-01-03",
        filesize: 2268979,
      },
    ],
    folderData: [
      {
        path: "netapp/folder-1-0/",
      },
      {
        path: "netapp/folder-1-1/",
      },
    ],
  },

  pagination: {
    maxKeys: 100,
    nextContinuationToken: null,
  },
};

export const getNetAppFolderResultsPlaywright = (path: string) => {
  if (!path || path === "netapp/") return netAppRootFolderResultsPlaywright;

  const levels = path.split("/").filter((part) => part.length > 0);
  if (levels.length > 3) {
    return {
      ...netAppRootFolderResultsPlaywright,
      data: {
        folderData: [],
        fileData: [],
      },
    };
  }
  const newFolders = netAppRootFolderResultsPlaywright.data.folderData.map(
    (item, index) => {
      return {
        ...item,
        path: `${path}folder-${levels.length}-${index}/`,
      };
    },
  );
  const newFiles = netAppRootFolderResultsPlaywright.data.fileData.map(
    (item, index) => {
      return { ...item, path: `${path}files-${levels.length}-${index}.pdf` };
    },
  );

  return {
    ...netAppRootFolderResultsPlaywright,
    data: {
      folderData: newFolders,
      fileData: newFiles,
    },
  };
};
