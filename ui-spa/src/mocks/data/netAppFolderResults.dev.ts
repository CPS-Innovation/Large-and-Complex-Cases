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
  console.log("path>>>>", path);
  if (!path || path === "abc") return netAppRootFolderResultsDev;
  const newData = netAppRootFolderResultsDev.data.map((item, index) => {
    return { ...item, folderPath: `${path}/folder_${index}` };
  });

  console.log("netAppRootFolderResultsDev>>", netAppRootFolderResultsDev);
  return {
    ...netAppRootFolderResultsDev,
    data: newData,
  };
};
