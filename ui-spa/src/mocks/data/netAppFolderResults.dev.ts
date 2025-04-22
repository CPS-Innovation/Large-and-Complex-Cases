import { NetAppFolderResponse } from "../../common/types/NetAppFolderData";
export const netAppRootFolderResultsDev: NetAppFolderResponse = {
  data: [
    {
      folderPath: "/abc/thunderstrikeab",
      caseId: 123,
    },
    {
      folderPath: "/abc/thunderstrike",
      caseId: null,
    },

    {
      folderPath: "/abc/thunderstrikeabc",
      caseId: null,
    },
  ],

  pagination: {
    maxKeys: 100,
    nextContinuationToken: null,
  },
};

export const getNetAppFolderResultsDev = (path: string) => {
  if (!path) return netAppRootFolderResultsDev;
  const newData = netAppRootFolderResultsDev.data.map((item, index) => {
    item.folderPath = `${path}/folder_${index}`;
    return item;
  });

  return {
    ...netAppRootFolderResultsDev,
    data: newData,
  };
};
