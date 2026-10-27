import { type EgressSearchResultResponse } from "../../schemas";
export const egressSearchResultsDev: EgressSearchResultResponse = {
  data: [
    {
      id: "1",
      dateCreated: "2000-01-25",
      name: "thunderstrike",
      caseId: null,
      connectedCaseUrn: null,
      connectedCaseName: null,
    },
    {
      id: "2",
      dateCreated: "2000-01-26",
      name: "thunderstrikeab",
      caseId: 123,
      connectedCaseUrn: "16XL2628726",
      connectedCaseName: "Thunderstrike1 Case",
    },
    {
      id: "3",
      dateCreated: "2000-01-27",
      name: "thunderstrikeabc",
      caseId: null,
      connectedCaseUrn: null,
      connectedCaseName: null,
    },
    {
      id: "4",
      dateCreated: "2000-01-28",
      name: "ahunderstrikeabcd",
      caseId: null,
      connectedCaseUrn: null,
      connectedCaseName: null,
    },
  ],

  pagination: {
    totalResults: 50,
    skip: 0,
    take: 50,
    count: 25,
  },
};
