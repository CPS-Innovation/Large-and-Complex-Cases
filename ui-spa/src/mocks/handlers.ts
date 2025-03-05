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
        caseId: 222,
        leadDefendantName: "abc2",
        operationName: "Thunderstruck",
        urn: "ABCDEF2",
        dateCreated: "02/01/2000",
      },
      {
        caseId: 333,
        leadDefendantName: "abc3",
        operationName: "Thunderstruck Traffic",
        urn: "ABCDEF3",
        dateCreated: "02/01/2000",
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
