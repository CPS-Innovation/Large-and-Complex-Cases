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
  egressToNetAppIndexingTransferDev,
  egressToNetAppIndexingTransferPlaywright,
  netAppToEgressIndexingTransferDev,
  netAppToEgressIndexingTransferPlaywright,
  egressToNetAppTransferStatusDev,
  egressToNetAppTransferStatusPlaywright,
  netAppToEgressTransferStatusDev,
  netAppToEgressTransferStatusPlaywright,
  // egressToNetAppIndexingErrorDev,
  activityLogDev,
  activityLogPlaywright,
} from "./data";
import { IndexingFileTransferPayload } from "../common/types/IndexingFileTransferPayload";
import { InitiateFileTransferPayload } from "../common/types/InitiateFileTransferPayload";

export const setupHandlers = (baseUrl: string, apiMockSource: string) => {
  const isDevMock = () => apiMockSource === "dev";
  const RESPONSE_DELAY = isDevMock() ? 10 : 0;
  return [
    http.get(`${baseUrl}/api/v1/areas`, async () => {
      const results = isDevMock() ? caseAreasDev : caseAreasPlaywright;
      await delay(RESPONSE_DELAY);
      return HttpResponse.json(results);
    }),

    http.get(`${baseUrl}/api/v1/case-search`, async () => {
      const caseSearchResults = isDevMock()
        ? casesSearchResultsDev
        : casesSearchResultsPlaywright;
      await delay(RESPONSE_DELAY);
      return HttpResponse.json(caseSearchResults);
    }),

    http.get(`${baseUrl}/api/v1/egress/workspaces`, async () => {
      const egressSearchResults = isDevMock()
        ? egressSearchResultsDev
        : egressSearchResultsPlaywright;
      await delay(RESPONSE_DELAY);

      return HttpResponse.json(egressSearchResults);
    }),

    http.post(`${baseUrl}/api/v1/egress/connections`, async () => {
      return HttpResponse.json({});
    }),

    http.get(`${baseUrl}/api/v1/netapp/folders`, async (req) => {
      const url = new URL(req.request.url);

      const path = url.searchParams.get("path");
      const netAppRootFolderResults = isDevMock()
        ? getConnectNetAppFolderResultsDev(path as string)
        : getConnectNetAppFolderResultsPlaywright(path as string);
      await delay(500);

      return HttpResponse.json(netAppRootFolderResults);
    }),

    http.post(`${baseUrl}/api/v1/netapp/connections`, async () => {
      return HttpResponse.json({});
    }),

    http.get(`${baseUrl}/api/v1/cases/12`, async () => {
      const caseMetaDataResults = isDevMock()
        ? caseMetaDataDev
        : caseMetaDataPlaywright;
      await delay(100);

      return HttpResponse.json(caseMetaDataResults);
    }),

    http.get(
      `${baseUrl}/api/v1/egress/workspaces/egress_1/files`,
      async (req) => {
        const url = new URL(req.request.url);

        const folderId = url.searchParams.get("folder-id");
        const netAppRootFolderResults = isDevMock()
          ? getEgressFolderResultsDev(folderId as string)
          : getEgressFolderResultsPlaywright(folderId as string);
        await delay(500);

        return HttpResponse.json(netAppRootFolderResults);
      },
    ),

    http.get(`${baseUrl}/api/v1/netapp/files`, async (req) => {
      const url = new URL(req.request.url);

      const path = url.searchParams.get("path");
      const netAppRootFolderResults = isDevMock()
        ? getNetAppFolderResultsDev(path as string)
        : getNetAppFolderResultsPlaywright(path as string);
      await delay(500);
      return HttpResponse.json(netAppRootFolderResults);
    }),

    http.post(`${baseUrl}/api/v1/filetransfer/files`, async ({ request }) => {
      const requestPayload =
        (await request.json()) as IndexingFileTransferPayload;
      let response = {};
      if (requestPayload.transferDirection === "EgressToNetApp") {
        response = isDevMock()
          ? egressToNetAppIndexingTransferDev
          : egressToNetAppIndexingTransferPlaywright;
      }
      if (requestPayload.transferDirection === "NetAppToEgress") {
        response = isDevMock()
          ? netAppToEgressIndexingTransferDev
          : netAppToEgressIndexingTransferPlaywright;
      }
      await delay(2500);
      return HttpResponse.json(response);
    }),

    http.post(
      `${baseUrl}/api/v1/filetransfer/initiate`,
      async ({ request }) => {
        const requestPayload =
          (await request.json()) as InitiateFileTransferPayload;
        let response = {};
        await delay(2000);
        response =
          requestPayload.transferDirection === "EgressToNetApp"
            ? { id: "transfer-id-egress-to-netapp" }
            : { id: "transfer-id-netapp-to-egress" };

        return HttpResponse.json(response);
      },
    ),
    http.get(
      `${baseUrl}/api/v1/filetransfer/transfer-id-egress-to-netapp/status`,
      async () => {
        const response = isDevMock()
          ? egressToNetAppTransferStatusDev
          : egressToNetAppTransferStatusPlaywright;

        await delay(500);
        return HttpResponse.json(response);
      },
    ),
    http.get(
      `${baseUrl}/api/v1/filetransfer/transfer-id-netapp-to-egress/status`,
      async () => {
        const response = isDevMock()
          ? netAppToEgressTransferStatusDev
          : netAppToEgressTransferStatusPlaywright;

        await delay(1500);
        return HttpResponse.json(response);
      },
    ),
    http.get(`${baseUrl}/api/v1/activity/logs`, async () => {
      const response = isDevMock() ? activityLogDev : activityLogPlaywright;

      await delay(1500);
      return HttpResponse.json(response);
    }),

    http.post(
      `${baseUrl}/api/v1/filetransfer/transfer-id-egress-to-netapp/clear`,
      async () => {
        return HttpResponse.json({});
      },
    ),
    http.post(
      `${baseUrl}/api/v1/filetransfer/transfer-id-netapp-to-egress/clear`,
      async () => {
        return HttpResponse.json({});
      },
    ),
    http.get(`${baseUrl}/api/v1/activity/*/logs/download`, async () => {
      const mockBlob = new Blob(["hello"], { type: "text/csv" });

      await delay(100);
      return new HttpResponse(mockBlob, {
        status: 200,
        headers: { "Content-Type": "text/csv" },
      });
    }),
  ];
};

type caseSearchResult = {
  caseId: number;
  leadDefendantName: string;
  urn: string;
};
export type caseSearchResults = caseSearchResult[];
