import { EgressSearchResultResponse } from "../../common/types/EgressSearchResponse";
export const egressSearchResultsDev: EgressSearchResultResponse = {
  data: [
    {
      id: "1",
      dateCreated: "2000-01-25",
      name: "thunderstrike",
      caseId: null,
    },
    {
      id: "2",
      dateCreated: "2000-01-26",
      name: "thunderstrikeab",
      caseId: null,
    },
    {
      id: "3",
      dateCreated: "2000-01-27",
      name: "thunderstrikeabc",
      caseId: null,
    },
    {
      id: "4",
      dateCreated: "2000-01-28",
      name: "ahunderstrikeabcd",
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
