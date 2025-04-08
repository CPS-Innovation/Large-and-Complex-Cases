import { EgressSearchResultResponse } from "../../common/types/EgressSearchResponse";
export const egressSearchResultsPlaywright: EgressSearchResultResponse = {
  data: [
    {
      id: "1",
      dateCreated: "2000-01-02",
      name: "thunderstrike",
      caseId: null,
    },
    {
      id: "2",
      dateCreated: "2000-01-02",
      name: "thunderstrike1",
      caseId: null,
    },
  ],

  pagination: {
    totalResults: 100,
    skip: 0,
    take: 50,
    count: 25,
  },
};
