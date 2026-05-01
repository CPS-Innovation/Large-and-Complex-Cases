import { v4 as uuidv4 } from "uuid";
import { z } from "zod";
import { GATEWAY_BASE_URL, GATEWAY_SCOPE } from "../config";
import { getAccessToken } from "../auth";
import {
  type CaseDivisionsOrAreaResponse,
  type SearchResultData,
  type EgressSearchResultData,
  type EgressSearchResultResponse,
  type ConnectNetAppFolder,
  type ConnectNetAppFolderData,
  type ConnectNetAppFolderResponse,
  type CaseMetaDataResponse,
  type EgressFolderData,
  type EgressFolderResponse,
  type NetAppFolder,
  type NetAppFile,
  type NetAppFolderResponse,
  type NetAppFolderDataResponse,
  type IndexingFileTransferResponse,
  type InitiateFileTransferResponse,
  type TransferStatusResponse,
  type ActivityLogResponse,
  type IndexingFileTransferPayload,
  type InitiateFileTransferPayload,
  caseDivisionsOrAreaResponseSchema,
  searchResultDataSchema,
  egressSearchResultResponseSchema,
  connectNetAppFolderResponseSchema,
  caseMetaDataResponseSchema,
  egressFolderResponseSchema,
  netAppFolderResponseSchema,
  indexingFileTransferResponseSchema,
  initiateFileTransferResponseSchema,
  transferStatusResponseSchema,
  activityLogResponseSchema,
  // indexingFileTransferPayloadSchema,
  // initiateFileTransferPayloadSchema,
} from "../schemas";
import { type CaseSearchParams } from "../common/types/CaseSearchParams";
import { ApiError } from "../common/errors/ApiError";

export const CORRELATION_ID = "Correlation-Id";

const buildCommonHeaders = async (): Promise<Record<string, string>> => {
  return {
    [CORRELATION_ID]: uuidv4(),
    Authorization: `Bearer ${await getAccessToken([GATEWAY_SCOPE])}`,
  };
};

export const parseAndValidateResponse = async <T>(
  response: Response,
  url: string,
  schema: z.ZodType<T>,
  contextText: string,
): Promise<T> => {
  let parsedJson: unknown;
  try {
    parsedJson = await response.json();
  } catch (error) {
    throw new ApiError(`${error}`, url, response);
  }

  const result = schema.safeParse(parsedJson);

  if (!result.success) {
    console.warn(`${contextText} validation failed`, result.error);
    throw new ApiError(`response schema validation failed`, url, response);
  }

  return result.data;
};

export const getCaseSearchResults = async (
  searchParams: CaseSearchParams,
): Promise<SearchResultData> => {
  const params = new URLSearchParams(searchParams);
  const url = `${GATEWAY_BASE_URL}/api/v1/case-search?${params}`;

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
  const result = await parseAndValidateResponse<SearchResultData>(
    response,
    url,
    searchResultDataSchema,
    "searchResultDataSchema",
  );
  return result;
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

  const result = await parseAndValidateResponse<CaseDivisionsOrAreaResponse>(
    response,
    url,
    caseDivisionsOrAreaResponseSchema,
    "caseDivisionsOrAreaResponseSchema",
  );
  return result;
};

export const getEgressSearchResults = async (
  workspaceName: string,
  skip: number = 0,
  take: number = 50,
  collected: EgressSearchResultData = [],
): Promise<EgressSearchResultData> => {
  const url = `${GATEWAY_BASE_URL}/api/v1/egress/workspaces`;
  const params = new URLSearchParams({
    "workspace-name": workspaceName,
    skip: `${skip}`,
    take: `${take}`,
  });
  const response = await fetch(`${url}?${params}`, {
    method: "GET",
    credentials: "include",
    headers: {
      ...(await buildCommonHeaders()),
    },
  });
  if (!response.ok) {
    throw new ApiError(`Searching for Egress workspaces failed`, url, response);
  }

  const result = await parseAndValidateResponse<EgressSearchResultResponse>(
    response,
    url,
    egressSearchResultResponseSchema,
    "egressSearchResultResponseSchema",
  );

  const { data, pagination } = result;
  const updated = collected.concat(data);
  if (skip + take >= pagination.totalResults) {
    return updated;
  }
  return getEgressSearchResults(workspaceName, skip + take, take, updated);
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
  const params = new URLSearchParams({
    "operation-name": operationName,
    path: folderPath,
    take: `${take}`,
    "continuation-token": continuationToken,
  });
  const response = await fetch(`${url}?${params}`, {
    method: "GET",
    credentials: "include",
    headers: {
      ...(await buildCommonHeaders()),
    },
  });
  if (!response.ok) {
    throw new ApiError(`getting netapp folders failed`, url, response);
  }
  const result = await parseAndValidateResponse<ConnectNetAppFolderResponse>(
    response,
    url,
    connectNetAppFolderResponseSchema,
    "connectNetAppFolderResponseSchema",
  );

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
  const result = await parseAndValidateResponse<CaseMetaDataResponse>(
    response,
    url,
    caseMetaDataResponseSchema,
    "caseMetaDataResponseSchema",
  );
  return result;
};

export const getEgressFolders = async (
  workspaceId: string,
  folderId: string,
  skip: number = 0,
  take: number = 50,
  collected: EgressFolderData = [],
): Promise<EgressFolderData> => {
  const params = new URLSearchParams({
    "folder-id": folderId,
    skip: `${skip}`,
    take: `${take}`,
  });
  const url = `${GATEWAY_BASE_URL}/api/v1/egress/workspaces/${workspaceId}/files?${params}`;
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

  const result = await parseAndValidateResponse<EgressFolderResponse>(
    response,
    url,
    egressFolderResponseSchema,
    "egressFolderResponseSchema",
  );

  const { data, pagination } = result;
  const updated = collected.concat(data);
  if (skip + take >= pagination.totalResults) {
    return updated;
  }
  return getEgressFolders(workspaceId, folderId, skip + take, take, updated);
};

export const getNetAppFolders = async (
  folderPath: string,
  take: number = 50,
  continuationToken = "",
  collectedFolders: NetAppFolder[] = [],
  collectedFiles: NetAppFile[] = [],
): Promise<NetAppFolderDataResponse> => {
  const url = `${GATEWAY_BASE_URL}/api/v1/netapp/files`;
  const params = new URLSearchParams({
    path: folderPath,
    take: `${take}`,
    "continuation-token": continuationToken,
  });
  const response = await fetch(`${url}?${params}`, {
    method: "GET",
    credentials: "include",
    headers: {
      ...(await buildCommonHeaders()),
    },
  });
  if (!response.ok) {
    throw new ApiError(`getting netapp files/folders failed`, url, response);
  }
  const result = await parseAndValidateResponse<NetAppFolderResponse>(
    response,
    url,
    netAppFolderResponseSchema,
    "netAppFolderResponseSchema",
  );

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

  const result = await parseAndValidateResponse<IndexingFileTransferResponse>(
    response,
    url,
    indexingFileTransferResponseSchema,
    "indexingFileTransferResponseSchema",
  );
  return result;
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

  const result = await parseAndValidateResponse<InitiateFileTransferResponse>(
    response,
    url,
    initiateFileTransferResponseSchema,
    "initiateFileTransferResponseSchema",
  );
  return result;
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
  const result = await parseAndValidateResponse<TransferStatusResponse>(
    response,
    url,
    transferStatusResponseSchema,
    "transferStatusResponseSchema",
  );
  return result;
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
  const params = new URLSearchParams({
    "case-id": caseId,
  });
  const url = `${GATEWAY_BASE_URL}/api/v1/activity/logs?${params}`;

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
  const result = await parseAndValidateResponse<ActivityLogResponse>(
    response,
    url,
    activityLogResponseSchema,
    "activityLogResponseSchema",
  );
  return result;
};

export const downloadActivityLog = async (activityId: string) => {
  const url = `${GATEWAY_BASE_URL}/api/v1/activity/${activityId}/logs/download`;

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
