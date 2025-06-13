import { http, delay, HttpResponse } from "msw";
import {
  caseAreasDev,
  caseAreasPlaywright,
  casesSearchResultsDev,
  casesSearchResultsPlaywright,
  egressSearchResultsDev,
  egressSearchResultsPlaywright,
  getConnectNetAppFolderResultsDev,
  getConnectNetAppFolderResultsPlaywright,
  caseMetaDataDev,
  caseMetaDataPlaywright,
  getEgressFolderResultsDev,
  getEgressFolderResultsPlaywright,
  getNetAppFolderResultsDev,
  getNetAppFolderResultsPlaywright,
} from "./data";

export const setupHandlers = (baseUrl: string, apiMockSource: string) => {
  const isDevMock = () => apiMockSource === "dev";
  const RESPONSE_DELAY = isDevMock() ? 10 : 0;
  return [
    http.get(`${baseUrl}/api/areas`, async () => {
      const results = isDevMock() ? caseAreasDev : caseAreasPlaywright;
      await delay(RESPONSE_DELAY);
      return HttpResponse.json(results);
    }),

    http.get(`${baseUrl}/api/case-search`, async () => {
      const caseSearchResults = isDevMock()
        ? casesSearchResultsDev
        : casesSearchResultsPlaywright;
      await delay(RESPONSE_DELAY);
      return HttpResponse.json(caseSearchResults);
    }),

    http.get(`${baseUrl}/api/egress/workspaces`, async () => {
      const egressSearchResults = isDevMock()
        ? egressSearchResultsDev
        : egressSearchResultsPlaywright;
      await delay(RESPONSE_DELAY);

      return HttpResponse.json(egressSearchResults);
    }),

    http.post(`${baseUrl}/api/egress/connections`, async () => {
      return HttpResponse.json({});
    }),

    http.get(`${baseUrl}/api/netapp/folders`, async (req) => {
      const url = new URL(req.request.url);

      const path = url.searchParams.get("path");
      const netAppRootFolderResults = isDevMock()
        ? getConnectNetAppFolderResultsDev(path as string)
        : getConnectNetAppFolderResultsPlaywright(path as string);
      await delay(500);

      return HttpResponse.json(netAppRootFolderResults);
    }),

    http.post(`${baseUrl}/api/netapp/connections`, async () => {
      return HttpResponse.json({});
    }),

    http.get(`${baseUrl}/api/cases/12`, async () => {
      const caseMetaDataResults = isDevMock()
        ? caseMetaDataDev
        : caseMetaDataPlaywright;
      await delay(100);

      return HttpResponse.json(caseMetaDataResults);
    }),

    http.get(`${baseUrl}/api/egress/workspaces/egress_1/files`, async (req) => {
      const url = new URL(req.request.url);

      const folderId = url.searchParams.get("folder-id");
      const netAppRootFolderResults = isDevMock()
        ? getEgressFolderResultsDev(folderId as string)
        : getEgressFolderResultsPlaywright(folderId as string);
      await delay(500);

      return HttpResponse.json(netAppRootFolderResults);
    }),

    http.get(`${baseUrl}/api/netapp/files`, async (req) => {
      const url = new URL(req.request.url);

      const path = url.searchParams.get("path");
      const netAppRootFolderResults = isDevMock()
        ? getNetAppFolderResultsDev(path as string)
        : getNetAppFolderResultsPlaywright(path as string);
      await delay(500);
      return HttpResponse.json(netAppRootFolderResults);
    }),

    http.post(`${baseUrl}/api/v1/filetransfer/validate`, async () => {
      await delay(2500);
      return HttpResponse.json({ isValid: true });
    }),

    http.post(`${baseUrl}/api/v1/filetransfer/initiate`, async () => {
      return HttpResponse.json({ transferId: "transfer-id-1" });
    }),
    http.get(
      `${baseUrl}/api/v1/filetransfer/transfer-id-1/status`,
      async () => {
        // const url = new URL(req.request.url);

        // const path = url.searchParams.get("path");
        // const netAppRootFolderResults = isDevMock()
        //   ? getNetAppFolderResultsDev(path as string)
        //   : getNetAppFolderResultsPlaywright(path as string);
        await delay(1500);
        return HttpResponse.json({ overallStatus: "COMPLETED" });
      },
    ),
  ];
};

type caseSearchResult = {
  caseId: number;
  leadDefendantName: string;
  urn: string;
};
export type caseSearchResults = caseSearchResult[];
