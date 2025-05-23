import { EgressSearchResultResponse } from "../../common/types/EgressSearchResponse";
export const egressSearchResultsPlaywright: EgressSearchResultResponse = {
  data: [
    {
      id: "1",
      dateCreated: "2000-01-03",
      name: "thunderstrike",
      caseId: null,
    },
    {
      id: "2",
      dateCreated: "2000-01-02",
      name: "thunderstrike1",
      caseId: 123,
    },
  ],

  pagination: {
    totalResults: 50,
    skip: 0,
    take: 50,
    count: 25,
  },
};
