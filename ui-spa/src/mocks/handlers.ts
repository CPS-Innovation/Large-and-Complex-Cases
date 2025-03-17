import { http, delay, HttpResponse } from "msw";
import { GATEWAY_BASE_URL } from "../config";
import {
  caseAreasDev,
  caseAreasPlaywright,
  casesSearchResultsDev,
  casesSearchResultsPlaywright,
} from "./data";

import { MockApiConfig } from "../mocks/browser";

export const setupHandlers = ({ sourceName }: MockApiConfig) => {
  const RESPONSE_DELAY = sourceName === "dev" ? 10 : 0;
  return [
    http.get(`${GATEWAY_BASE_URL}/api/areas`, async () => {
      const results = sourceName === "dev" ? caseAreasDev : caseAreasPlaywright;
      await delay(RESPONSE_DELAY);
      return HttpResponse.json(results);
    }),

    http.get(`${GATEWAY_BASE_URL}/api/search-results`, async () => {
      const caseSearchResults =
        sourceName === "dev"
          ? casesSearchResultsDev
          : casesSearchResultsPlaywright;
      await delay(RESPONSE_DELAY);
      return HttpResponse.json(caseSearchResults);
    }),
  ];
};

type caseSearchResult = {
  caseId: number;
  leadDefendantName: string;
  urn: string;
};
export type caseSearchResults = caseSearchResult[];
