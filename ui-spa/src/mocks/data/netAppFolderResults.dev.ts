import { NetAppFolderResponse } from "../../common/types/NetAppFolderData";
export const netAppFolderResultsDev: NetAppFolderResponse = {
  data: [
    {
      folderPath: "/abc/thunderstrike",
      caseId: null,
    },
    {
      folderPath: "/abc/thunderstrikeab",
      caseId: 123,
    },
    {
      folderPath: "/abc/thunderstrikeabc",
      caseId: null,
    },
    {
      folderPath: "/abc/ahunderstrikeabcd",
      caseId: null,
    },
  ],

  pagination: {
    totalResults: 50,
    skip: 0,
    take: 50,
    count: 25,
  },
};
