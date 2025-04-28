import { NetAppFolderResponse } from "../../common/types/NetAppFolderData";
export const netAppRootFolderResultsPlaywright: NetAppFolderResponse = {
  data: {
    rootPath: "",
    folders: [
      {
        folderPath: "thunderstrike/",
        caseId: null,
      },
      {
        folderPath: "thunderstrikeab/",
        caseId: 123,
      },
    ],
  },
  pagination: {
    maxKeys: 100,
    nextContinuationToken: null,
  },
};

export const getNetAppFolderResultsPlaywright = (path: string) => {
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
      return { ...item, folderPath: `${path}folder-${index}/` };
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
