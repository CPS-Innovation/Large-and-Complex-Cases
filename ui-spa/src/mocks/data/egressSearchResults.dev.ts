import { EgressSearchResultResponse } from "../../common/types/EgressSearchResponse";
export const egressSearchResultsDev: EgressSearchResultResponse = {
  data: [
    {
      id: "1",
      dateCreated: "2000-01-25",
      name: "thunderstrike",
    },
    {
      id: "2",
      dateCreated: "2000-01-26",
      name: "thunderstrikeab",
    },
    {
      id: "3",
      dateCreated: "2000-01-27",
      name: "thunderstrikeabc",
    },
    {
      id: "4",
      dateCreated: "2000-01-28",
      name: "ahunderstrikeabcd",
    },
  ],

  pagination: {
    totalResults: 50,
    skip: 0,
    take: 50,
    count: 25,
  },
};
