import { v4 as uuidv4 } from "uuid";
import { GATEWAY_BASE_URL, GATEWAY_SCOPE } from "../config";
import { getAccessToken } from "../auth";
import { CaseDivisionsOrAreaResponse } from "../common/types/LooksupData";
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
    throw new Error(
      `case-search api failed with status: ${response.status}, method:GET`,
    );
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
    throw new Error(
      `areas api failed with status: ${response.status}, method:GET`,
    );
  }
  return (await response.json()) as CaseDivisionsOrAreaResponse;
};

export const getEgressSearchResults = async (searchParams: string) => {
  const url = `${GATEWAY_BASE_URL}/api/egress/workspace-name?${searchParams}`;

  const response = await fetch(url, {
    method: "GET",
    credentials: "include",
    headers: {
      ...(await buildCommonHeaders()),
    },
  });

  if (!response.ok) {
    throw new Error(
      `egress search api failed with status: ${response.status}, method:GET`,
    );
  }
  return await response.json();
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
    throw new ApiError(`Connecting to Egress workspace failed.`, url, response);
  }
  return response;
};
