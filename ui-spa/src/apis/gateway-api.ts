import { v4 as uuidv4 } from "uuid";
import { GATEWAY_BASE_URL, GATEWAY_SCOPE } from "../config";
import { getAccessToken } from "../auth";
import { CaseDivisionsOrAreaResponse } from "../common/types/LooksupData";
import { SearchResultData } from "../common/types/SearchResultResponse";
import {
  EgressSearchResultData,
  EgressSearchResultResponse,
} from "../common/types/EgressSearchResponse";
import {
  NetAppFolder,
  NetAppFolderData,
  NetAppFolderResponse,
} from "../common/types/NetAppFolderData";
import { CaseMetaDataResponse } from "../common/types/CaseMetaDataResponse";

import {
  EgressFolderData,
  EgressFolderResponse,
} from "../common/types/EgressFolderData";

import { ApiError } from "../common/errors/ApiError";

export const CORRELATION_ID = "Correlation-Id";

const buildCommonHeaders = async (): Promise<Record<string, string>> => {
  return {
    [CORRELATION_ID]: uuidv4(),
    Authorization: `Bearer ${await getAccessToken([GATEWAY_SCOPE])}`,
  };
};

export const getCaseSearchResults = async (
  searchParams: string,
): Promise<SearchResultData> => {
  const url = `${GATEWAY_BASE_URL}/api/case-search?${searchParams}`;

  const response = await fetch(url, {
    method: "GET",
    credentials: "include",
    headers: {
      ...(await buildCommonHeaders()),
    },
  });

  if (!response.ok) {
    throw new ApiError(`Searching for cases failed`, url, response);
  }
  return await response.json();
};

export const getCaseDivisionsOrAreas = async () => {
  const url = `${GATEWAY_BASE_URL}/api/areas`;

  const response = await fetch(url, {
    method: "GET",
    credentials: "include",
    headers: {
      ...(await buildCommonHeaders()),
    },
  });

  if (!response.ok) {
    throw new ApiError(`Getting case areas failed`, url, response);
  }
  return (await response.json()) as CaseDivisionsOrAreaResponse;
};

export const getEgressSearchResults = async (
  searchParams: string,
  skip: number = 0,
  take: number = 50,
  collected: EgressSearchResultData = [],
): Promise<EgressSearchResultData> => {
  const url = `${GATEWAY_BASE_URL}/api/egress/workspaces`;
  const response = await fetch(
    `${url}?${searchParams}&skip=${skip}&take=${take}`,
    {
      method: "GET",
      credentials: "include",
      headers: {
        ...(await buildCommonHeaders()),
      },
    },
  );
  if (!response.ok) {
    throw new ApiError(`Searching for Egress workspaces failed`, url, response);
  }
  try {
    const result = (await response.json()) as EgressSearchResultResponse;

    const { data, pagination } = result;
    const updated = collected.concat(data);
    if (skip + take >= pagination.totalResults) {
      return updated;
    }
    return getEgressSearchResults(searchParams, skip + take, take, updated);
  } catch (error) {
    console.error("Fetch failed:", error);
    throw new Error(
      `Invalid API response format for Egress workspace search results, ${error}`,
    );
  }
};

export const connectEgressWorkspace = async ({
  workspaceId,
  caseId,
}: {
  workspaceId: string;
  caseId: string;
}) => {
  const url = `${GATEWAY_BASE_URL}/api/egress/connections`;

  const response = await fetch(url, {
    method: "POST",
    credentials: "include",
    headers: {
      ...(await buildCommonHeaders()),
    },
    body: JSON.stringify({
      egressWorkspaceId: workspaceId,
      caseId: parseInt(caseId),
    }),
  });

  if (!response.ok) {
    throw new ApiError(`Connecting to Egress workspace failed`, url, response);
  }
  return response;
};

export const getNetAppFolders = async (
  operationName: string,
  folderPath: string,
  take: number = 50,
  continuationToken = "",
  collectedFolders: NetAppFolder[] = [],
): Promise<NetAppFolderData> => {
  const url = `${GATEWAY_BASE_URL}/api/netapp/folders`;
  const response = await fetch(
    `${url}?operation-name=${operationName}&path=${folderPath}&take=${take}&continuation-token=${continuationToken}`,
    {
      method: "GET",
      credentials: "include",
      headers: {
        ...(await buildCommonHeaders()),
      },
    },
  );
  if (!response.ok) {
    throw new ApiError(`getting netapp folders failed`, url, response);
  }
  try {
    const result = (await response.json()) as NetAppFolderResponse;

    const { data, pagination } = result;
    const updatedFolders = collectedFolders.concat(data.folders);
    if (!pagination.nextContinuationToken) {
      return {
        rootPath: data.rootPath,
        folders: updatedFolders,
      };
    }
    return getNetAppFolders(
      operationName,
      folderPath,
      take,
      pagination.nextContinuationToken,
      updatedFolders,
    );
  } catch (error) {
    console.error("Fetch failed:", error);
    throw new Error(
      `Invalid API response format for netapp folders results, ${error}`,
    );
  }
};

export const connectNetAppFolder = async ({
  operationName,
  folderPath,
  caseId,
}: {
  operationName: string;
  folderPath: string;
  caseId: string;
}) => {
  const url = `${GATEWAY_BASE_URL}/api/netapp/connections`;

  const response = await fetch(url, {
    method: "POST",
    credentials: "include",
    headers: {
      ...(await buildCommonHeaders()),
    },
    body: JSON.stringify({
      operationName: operationName,
      folderPath: folderPath,
      caseId: parseInt(caseId),
    }),
  });

  if (!response.ok) {
    throw new ApiError(`Connecting to NetApp folder failed`, url, response);
  }
  return response;
};

export const getCaseMetaData = async (caseId: string) => {
  const url = `${GATEWAY_BASE_URL}/api/cases/${caseId}`;

  const response = await fetch(url, {
    method: "GET",
    credentials: "include",
    headers: {
      ...(await buildCommonHeaders()),
    },
  });

  if (!response.ok) {
    throw new ApiError(`Getting case metadata failed`, url, response);
  }
  return (await response.json()) as CaseMetaDataResponse;
};

export const getEgressFolders = async (
  workspaceId: string,
  folderId: string,
  skip: number = 0,
  take: number = 50,
  collected: EgressFolderData = [],
): Promise<EgressFolderData> => {
  const url = `${GATEWAY_BASE_URL}/api/egress/workspaces/${workspaceId}/files?folder-id=${folderId}&skip=${skip}&take=${take}`;
  const response = await fetch(url, {
    method: "GET",
    credentials: "include",
    headers: {
      ...(await buildCommonHeaders()),
    },
  });
  if (!response.ok) {
    throw new ApiError(`Getting egress folders failed`, url, response);
  }
  try {
    const result = (await response.json()) as EgressFolderResponse;

    const { data, pagination } = result;
    const updated = collected.concat(data);
    if (skip + take >= pagination.totalResults) {
      return updated;
    }
    return getEgressFolders(workspaceId, folderId, skip + take, take, updated);
  } catch (error) {
    console.error("Fetch failed:", error);
    throw new Error(`Invalid API response format for Egress folders, ${error}`);
  }
};
