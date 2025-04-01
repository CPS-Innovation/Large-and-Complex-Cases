import { v4 as uuidv4 } from "uuid";
import { GATEWAY_BASE_URL, GATEWAY_SCOPE } from "../config";
import { getAccessToken } from "../auth";
import { CaseDivisionsOrAreaResponse } from "../common/types/LooksupData";

export const CORRELATION_ID = "Correlation-Id";

const buildCommonHeaders = async (): Promise<Record<string, string>> => {
  return {
    [CORRELATION_ID]: uuidv4(),
    Authorization: `Bearer ${await getAccessToken([GATEWAY_SCOPE])}`,
  };
};

export const getCaseSearchResults = async (searchParams: string) => {
  try {
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
  } catch (error) {
    console.error("Error:", error);
    throw error;
  }
};

export const getCaseDivisionsOrAreas = async () => {
  try {
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
  } catch (error) {
    console.error("Error:", error);
    throw error;
  }
};

export const getEgressSearchResults = async (searchParams: string) => {
  try {
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
  } catch (error) {
    console.error("Error:", error);
    throw error;
  }
};
