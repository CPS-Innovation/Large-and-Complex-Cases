import { EgressSearchResultData } from "../../common/types/EgressSearchResponse";
export const egressSearchResultsDev: EgressSearchResultData = {
  data: [
    {
      id: "1",
      dateCreated: "2000-01-02",
      name: "thunderstrike",
    },
    {
      id: "2",
      dateCreated: "2000-01-02",
      name: "thunderstrike1",
    },
  ],

  pagination: {
    totalResults: 100,
    skip: 0,
    take: 50,
    count: 25,
  },
};
