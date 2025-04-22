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
  NetAppFolderData,
  NetAppFolderResponse,
} from "../common/types/NetAppFolderData";
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

export const getNetAppFolders = async (
  operationName: string,
  folderPath: string,
  take: number = 50,
  continuationToken = "",
  collected: NetAppFolderData = [],
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
    const updated = collected.concat(data);
    if (!pagination.nextContinuationToken) {
      return updated;
    }
    return getNetAppFolders(
      operationName,
      folderPath,
      take,
      pagination.nextContinuationToken,
      updated,
    );
  } catch (error) {
    console.error("Fetch failed:", error);
    throw new Error(
      `Invalid API response format for netapp folders results, ${error}`,
    );
  }
};
