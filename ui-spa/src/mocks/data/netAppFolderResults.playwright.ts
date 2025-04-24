import { NetAppFolderResponse } from "../../common/types/NetAppFolderData";
export const netAppRootFolderResultsPlaywright: NetAppFolderResponse = {
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

export const getNetAppFolderResultsPlaywright = (path: string) => {
  if (!path || path === "abc") return netAppRootFolderResultsPlaywright;
  const newData = netAppRootFolderResultsPlaywright.data.map((item, index) => {
    return { ...item, folderPath: `${path}/folder_${index}` };
  });

  return {
    ...netAppRootFolderResultsPlaywright,
    data: newData,
  };
};
