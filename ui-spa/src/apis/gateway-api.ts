import { v4 as uuidv4 } from "uuid";
import { GATEWAY_BASE_URL, GATEWAY_SCOPE } from "../config";
import { getAccessToken } from "../auth";
import { CaseDivisionsOrAreaResponse } from "../common/types/LooksupData";
import { EgressSearchResultData } from "../common/types/EgressSearchResponse";
import { ApiError } from "../common/errors/ApiError";

export const CORRELATION_ID = "Correlation-Id";

const buildCommonHeaders = async (): Promise<Record<string, string>> => {
  return {
    [CORRELATION_ID]: uuidv4(),
    Authorization: `Bearer ${await getAccessToken([GATEWAY_SCOPE])}`,
  };
};

export const getCaseSearchResults = async (searchParams: string) => {
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
    body: JSON.stringify({ workspaceId, caseId }),
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
  const url = `${GATEWAY_BASE_URL}/api/egress/workspace-name`;
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
    const result = await response.json();

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
