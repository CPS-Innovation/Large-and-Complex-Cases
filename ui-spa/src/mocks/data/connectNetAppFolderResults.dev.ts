import { ConnectNetAppFolderResponse } from "../../common/types/ConnectNetAppFolderData";
export const netAppRootFolderResultsDev: ConnectNetAppFolderResponse = {
  data: {
    rootPath: "",
    folders: [
      {
        folderPath: "thunderstrikeab/",
        caseId: 123,
      },
      {
        folderPath: "thunderstrike/",
        caseId: null,
      },

      {
        folderPath: "thunderstrikeabc/",
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
      return { ...item, folderPath: `${path}folder-${index}/` };
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
