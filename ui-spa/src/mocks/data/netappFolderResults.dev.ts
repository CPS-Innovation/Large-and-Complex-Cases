import { NetAppFolderResponse } from "../../common/types/NetAppFolderData";
export const netAppRootFolderResultsDev: NetAppFolderResponse = {
  data: {
    fileData: [
      {
        path: "netapp/file-1-0.pdf",
        lastModified: "2000-01-02",
        filesize: 1234,
      },
      {
        path: "eweweweweewweewwewewewewewewweewerwrrwwrwrrrrrrwrwrwrwweewweeewweweeweweweew.pdf",
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

      {
        path: "netapp/folder-1-2/",
      },
      {
        path: "netapp/folder-1-0/",
      },
      {
        path: "netapp/folder-1-1/",
      },

      {
        path: "netapp/folder-1-2/",
      },
      {
        path: "netapp/folder-1-0/",
      },
      {
        path: "netapp/folder-1-1/",
      },

      {
        path: "netapp/folder-1-2/",
      },
      {
        path: "netapp/folder-1-0/",
      },
      {
        path: "netapp/folder-1-1/",
      },

      {
        path: "netapp/folder-1-2/",
      },
      {
        path: "netapp/folder-1-0/",
      },
      {
        path: "netapp/folder-1-1/",
      },

      {
        path: "netapp/folder-1-2/",
      },
    ],
  },

  pagination: {
    maxKeys: 100,
    nextContinuationToken: null,
  },
};

export const getNetAppFolderResultsDev = (path: string) => {
  if (!path || path === "netapp/") return netAppRootFolderResultsDev;

  const levels = path.split("/").filter((part) => part.length > 0);
  if (levels.length > 3) {
    return {
      ...netAppRootFolderResultsDev,
      data: {
        folderData: [],
        fileData: [],
      },
    };
  }
  const newFolders = netAppRootFolderResultsDev.data.folderData.map(
    (item, index) => {
      return {
        ...item,
        path: `${path}folder-${levels.length}-${index}/`,
      };
    },
  );
  const newFiles = netAppRootFolderResultsDev.data.fileData.map(
    (item, index) => {
      return { ...item, path: `${path}files-${levels.length}-${index}.pdf` };
    },
  );

  return {
    ...netAppRootFolderResultsDev,
    data: {
      folderData: newFolders,
      fileData: newFiles,
    },
  };
};
