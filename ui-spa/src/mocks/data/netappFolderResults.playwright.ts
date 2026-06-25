import { type NetAppFolderResponse } from "../../schemas";
export const netAppRootFolderResultsPlaywright: NetAppFolderResponse = {
  data: {
    fileData: [
      {
        path: "netapp/netapp-file-1-0.pdf",
        lastModified: "2000-01-02",
        filesize: 1234,
      },
    ],
    folderData: [
      {
        path: "netapp/netapp-folder-1-0/",
      },
      {
        path: "netapp/netapp-folder-1-1/",
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
        path: `${path}netapp-folder-${levels.length}-${index}/`,
      };
    },
  );
  const newFiles = netAppRootFolderResultsPlaywright.data.fileData.map(
    (item, index) => {
      return {
        ...item,
        path: `${path}netapp-file-${levels.length}-${index}.pdf`,
      };
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
