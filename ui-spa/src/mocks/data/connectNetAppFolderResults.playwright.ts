import { ConnectNetAppFolderResponse } from "../../common/types/ConnectNetAppFolderData";
export const netAppRootFolderResultsPlaywright: ConnectNetAppFolderResponse = {
  data: {
    rootPath: "",
    folders: [
      {
        path: "thunderstrike/",
        caseId: null,
      },
      {
        path: "thunderstrikeab/",
        caseId: 123,
      },
    ],
  },
  pagination: {
    maxKeys: 100,
    nextContinuationToken: null,
  },
};

export const getConnectNetAppFolderResultsPlaywright = (path: string) => {
  if (!path || path === "abc") return netAppRootFolderResultsPlaywright;
  const levels = path.split("/").filter((part) => part.length > 0);
  if (levels.length > 3) {
    return {
      ...netAppRootFolderResultsPlaywright,
      data: {
        roothPath: path,
        folders: [],
      },
    };
  }
  const newFolders = netAppRootFolderResultsPlaywright.data.folders.map(
    (item, index) => {
      return { ...item, path: `${path}folder-${index}/` };
    },
  );

  return {
    ...netAppRootFolderResultsPlaywright,
    data: {
      roothPath: path,
      folders: newFolders,
    },
  };
};
