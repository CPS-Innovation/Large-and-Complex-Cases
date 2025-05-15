import { ConnectNetAppFolderResponse } from "../../common/types/ConnectNetAppFolderData";
export const netAppRootFolderResultsDev: ConnectNetAppFolderResponse = {
  data: {
    rootPath: "",
    folders: [
      {
        path: "thunderstrikeab/",
        caseId: 123,
      },
      {
        path: "thunderstrike/",
        caseId: null,
      },

      {
        path: "thunderstrikeabc/",
        caseId: null,
      },
    ],
  },

  pagination: {
    maxKeys: 100,
    nextContinuationToken: null,
  },
};

export const getConnectNetAppFolderResultsDev = (path: string) => {
  if (!path || path === "abc") return netAppRootFolderResultsDev;

  const levels = path.split("/").filter((part) => part.length > 0);
  if (levels.length > 3) {
    return {
      ...netAppRootFolderResultsDev,
      data: {
        roothPath: path,
        folders: [],
      },
    };
  }
  const newFolders = netAppRootFolderResultsDev.data.folders.map(
    (item, index) => {
      return { ...item, path: `${path}folder-${index}/` };
    },
  );

  return {
    ...netAppRootFolderResultsDev,
    data: {
      roothPath: path,
      folders: newFolders,
    },
  };
};
