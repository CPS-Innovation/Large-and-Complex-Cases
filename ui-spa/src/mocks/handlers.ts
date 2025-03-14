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
  return [
    http.get(`${GATEWAY_BASE_URL}/api/areas`, async () => {
      const results = sourceName === "dev" ? caseAreasDev : caseAreasPlaywright;
      await delay();
      return HttpResponse.json(results);
    }),

    http.get(`${GATEWAY_BASE_URL}/api/cases`, async () => {
      const caseSearchResults =
        sourceName === "dev"
          ? casesSearchResultsDev
          : casesSearchResultsPlaywright;
      await delay();
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
