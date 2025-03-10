import { http, HttpResponse } from "msw";
import { GATEWAY_BASE_URL } from "../config";

export const handlers = [
  http.get(`${GATEWAY_BASE_URL}/api/initialmessage`, () => {
    return HttpResponse.json({
      message: "Hello World!",
    });
  }),

  http.get(`${GATEWAY_BASE_URL}/api/cases`, () => {
    const caseSearchResults = [
      {
        operationName: "",
        urn: "ABCDEF1",
        leadDefendantName: "abc1",
        egressStatus: "connected",
        sharedDrive: "connected",
        dateCreated: "02/01/2000",
      },
      {
        operationName: "Thunderstruck",
        urn: "ABCDEF2",
        leadDefendantName: "abc2",
        egressStatus: "connected",
        sharedDrive: "connected",
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
