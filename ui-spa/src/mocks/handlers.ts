import { http, HttpResponse } from "msw";
import { GATEWAY_BASE_URL } from "../config";

const areas = [
  {
    code: "1",
    name: "SEOCID Int London and SE Div",
    type: "Large and Complex Case Divisions",
  },
  {
    code: "2",
    name: "Special Crime Division",
    type: "Large and Complex Case Divisions",
  },
  {
    code: "3",
    name: "Bedfordshire",
    type: "CPS Areas",
  },
  {
    code: "4",
    name: "Cambridgeshire",
    type: "CPS Areas",
  },
  {
    code: "5",
    name: "Cheshire",
    type: "CPS Areas",
  },
];

export const handlers = [
  http.get(`${GATEWAY_BASE_URL}/api/areas`, () => {
    return HttpResponse.json(areas);
  }),

  http.get(`${GATEWAY_BASE_URL}/api/cases`, () => {
    const caseSearchResults = [
      {
        operationName: "",
        urn: "ABCDEF1",
        leadDefendantName: "abc1",
        egressStatus: "connected",
        sharedDriveStatus: "inactive",
        dateCreated: "02/01/2000",
      },
      {
        operationName: "Thunderstruck",
        urn: "ABCDEF2",
        leadDefendantName: "abc2",
        egressStatus: "connected",
        sharedDriveStatus: "connected",
        dateCreated: "03/01/2000",
      },
    ];

    return HttpResponse.json(caseSearchResults);
  }),
];

type caseSearchResult = {
  caseId: number;
  leadDefendantName: string;
  urn: string;
};
export type caseSearchResults = caseSearchResult[];
