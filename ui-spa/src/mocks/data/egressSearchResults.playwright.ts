import { type EgressSearchResultResponse } from "../../schemas";
export const egressSearchResultsPlaywright: EgressSearchResultResponse = {
  data: [
    {
      id: "1",
      dateCreated: "2000-01-03",
      name: "thunderstrike",
      caseId: null,
      connectedCaseUrn: null,
      connectedCaseName: null,
    },
    {
      id: "2",
      dateCreated: "2000-01-02",
      name: "thunderstrike1",
      caseId: 123,
      connectedCaseUrn: "16XL2628726",
      connectedCaseName: "Thunderstrike1 Case",
    },
  ],

  pagination: {
    totalResults: 50,
    skip: 0,
    take: 50,
    count: 25,
  },
};
