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
  ConnectNetAppFolder,
  ConnectNetAppFolderData,
  ConnectNetAppFolderResponse,
} from "../common/types/ConnectNetAppFolderData";
import { CaseMetaDataResponse } from "../common/types/CaseMetaDataResponse";
import {
  EgressFolderData,
  EgressFolderResponse,
} from "../common/types/EgressFolderData";
import {
  NetAppFolder,
  NetAppFile,
  NetAppFolderResponse,
  NetAppFolderDataResponse,
} from "../common/types/NetAppFolderData";
import { IndexingFileTransferResponse } from "../common/types/IndexingFileTransferResponse";
import { IndexingFileTransferPayload } from "../common/types/IndexingFileTransferPayload";
import { InitiateFileTransferResponse } from "../common/types/InitiateFileTransferResponse";
import { InitiateFileTransferPayload } from "../common/types/InitiateFileTransferPayload";
import { TransferStatusResponse } from "../common/types/TransferStatusResponse";
import { ActivityLogResponse } from "../common/types/ActivityLogResponse";

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
  const url = `${GATEWAY_BASE_URL}/api/v1/case-search?${searchParams}`;

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
  const url = `${GATEWAY_BASE_URL}/api/v1/areas`;

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
  const url = `${GATEWAY_BASE_URL}/api/v1/egress/workspaces`;
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
  const url = `${GATEWAY_BASE_URL}/api/v1/egress/connections`;

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

export const getConnectNetAppFolders = async (
  operationName: string,
  folderPath: string,
  take: number = 50,
  continuationToken = "",
  collectedFolders: ConnectNetAppFolder[] = [],
): Promise<ConnectNetAppFolderData> => {
  const url = `${GATEWAY_BASE_URL}/api/v1/netapp/folders`;
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
    const result = (await response.json()) as ConnectNetAppFolderResponse;

    const { data, pagination } = result;
    const updatedFolders = collectedFolders.concat(data.folders);
    if (!pagination.nextContinuationToken) {
      return {
        rootPath: data.rootPath,
        folders: updatedFolders,
      };
    }
    return getConnectNetAppFolders(
      operationName,
      folderPath,
      take,
      pagination.nextContinuationToken,
      updatedFolders,
    );
  } catch (error) {
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
  const url = `${GATEWAY_BASE_URL}/api/v1/netapp/connections`;

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
  const url = `${GATEWAY_BASE_URL}/api/v1/cases/${caseId}`;

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
  const url = `${GATEWAY_BASE_URL}/api/v1/egress/workspaces/${workspaceId}/files?folder-id=${folderId}&skip=${skip}&take=${take}`;
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
    throw new Error(`Invalid API response format for Egress folders, ${error}`);
  }
};

export const getNetAppFolders = async (
  folderPath: string,
  take: number = 50,
  continuationToken = "",
  collectedFolders: NetAppFolder[] = [],
  collectedFiles: NetAppFile[] = [],
): Promise<NetAppFolderDataResponse> => {
  const url = `${GATEWAY_BASE_URL}/api/v1/netapp/files`;
  const response = await fetch(
    `${url}?path=${folderPath}&take=${take}&continuation-token=${continuationToken}`,
    {
      method: "GET",
      credentials: "include",
      headers: {
        ...(await buildCommonHeaders()),
      },
    },
  );
  if (!response.ok) {
    throw new ApiError(`getting netapp files/folders failed`, url, response);
  }
  try {
    const result = (await response.json()) as NetAppFolderResponse;

    const { data, pagination } = result;
    const updatedFolders = collectedFolders.concat(data.folderData);
    const updatedFiles = collectedFiles.concat(data.fileData);
    if (!pagination.nextContinuationToken) {
      return {
        folderData: updatedFolders,
        fileData: updatedFiles,
      };
    }
    return getNetAppFolders(
      folderPath,
      take,
      pagination.nextContinuationToken,
      updatedFolders,
      updatedFiles,
    );
  } catch (error) {
    throw new Error(
      `Invalid API response format for netapp files/folders results, ${error}`,
    );
  }
};

export const indexingFileTransfer = async (
  payload: IndexingFileTransferPayload,
) => {
  const url = `${GATEWAY_BASE_URL}/api/v1/filetransfer/files`;

  const response = await fetch(url, {
    method: "POST",
    credentials: "include",
    headers: {
      ...(await buildCommonHeaders()),
    },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    throw new ApiError(`indexing file transfer api failed`, url, response);
  }
  return (await response.json()) as IndexingFileTransferResponse;
};

export const initiateFileTransfer = async (
  payload: InitiateFileTransferPayload,
) => {
  const url = `${GATEWAY_BASE_URL}/api/v1/filetransfer/initiate`;

  const response = await fetch(url, {
    method: "POST",
    credentials: "include",
    headers: {
      ...(await buildCommonHeaders()),
    },
    body: JSON.stringify(payload),
  });

  if (!response.ok) {
    throw new ApiError(`initiate file transfer failed`, url, response);
  }
  return (await response.json()) as InitiateFileTransferResponse;
};

export const getTransferStatus = async (transferId: string) => {
  const url = `${GATEWAY_BASE_URL}/api/v1/filetransfer/${transferId}/status`;

  const response = await fetch(url, {
    method: "GET",
    credentials: "include",
    headers: {
      ...(await buildCommonHeaders()),
    },
  });

  if (!response.ok) {
    throw new ApiError(`Getting case transfer status failed`, url, response);
  }
  return (await response.json()) as TransferStatusResponse;
};

export const handleFileTransferClear = async (transferId: string) => {
  const url = `${GATEWAY_BASE_URL}/api/v1/filetransfer/${transferId}/clear`;

  const response = await fetch(url, {
    method: "POST",
    credentials: "include",
    headers: {
      ...(await buildCommonHeaders()),
    },
  });

  if (!response.ok) {
    throw new ApiError(`clear file transfer api failed`, url, response);
  }
};

export const getActivityLog = async (caseId: string) => {
  const url = `${GATEWAY_BASE_URL}/api/v1/activity/logs?caseId=${caseId}`;

  const response = await fetch(url, {
    method: "GET",
    credentials: "include",
    headers: {
      ...(await buildCommonHeaders()),
    },
  });

  if (!response.ok) {
    throw new ApiError(`Getting case activity log failed`, url, response);
  }
  return (await response.json()) as ActivityLogResponse;
};

export const downloadActivityLog = async (transferId: string) => {
  const url = `${GATEWAY_BASE_URL}/api/v1/activity/${transferId}/logs/download`;

  const response = await fetch(url, {
    method: "GET",
    credentials: "include",
    headers: {
      ...(await buildCommonHeaders()),
    },
  });

  if (!response.ok) {
    throw new ApiError(`Downloading activity log failed`, url, response);
  }
  return response;
};
