import { http, delay, HttpResponse } from "msw";
import {
  caseAreasDev,
  caseAreasPlaywright,
  casesSearchResultsDev,
  casesSearchResultsPlaywright,
  egressSearchResultsDev,
  egressSearchResultsPlaywright,
} from "./data";

export const setupHandlers = (baseUrl: string, apiMockSource: string) => {
  const isDevMock = () => apiMockSource === "dev";
  const RESPONSE_DELAY = isDevMock() ? 10 : 0;
  return [
    http.get(`${baseUrl}/api/areas`, async () => {
      const results = isDevMock() ? caseAreasDev : caseAreasPlaywright;
      await delay(RESPONSE_DELAY);
      return HttpResponse.json(results);
      // return new HttpResponse(null, { status: 500 });
    }),

    http.get(`${baseUrl}/api/case-search`, async () => {
      const caseSearchResults = isDevMock()
        ? casesSearchResultsDev
        : casesSearchResultsPlaywright;
      await delay(RESPONSE_DELAY);
      return HttpResponse.json(caseSearchResults);
      // return new HttpResponse(null, { status: 500 });
    }),

    http.get(`${baseUrl}/api/egress/workspace-name`, async () => {
      const egressSearchResults = isDevMock()
        ? egressSearchResultsDev
        : egressSearchResultsPlaywright;
      await delay(RESPONSE_DELAY);

      return HttpResponse.json(egressSearchResults);
      // return HttpResponse.json({
      //   data: [],
      //   pagination: {
      //     totalResults: 50,
      //     skip: 0,
      //     take: 50,
      //     count: 25,
      //   },
      // });
      // return new HttpResponse(null, { status: 500 });
    }),

    http.post(`${baseUrl}/api/egress/connections`, async () => {
      return HttpResponse.json({});
      // return new HttpResponse(null, { status: 500 });
    }),
  ];
};

type caseSearchResult = {
  caseId: number;
  leadDefendantName: string;
  urn: string;
};
export type caseSearchResults = caseSearchResult[];
