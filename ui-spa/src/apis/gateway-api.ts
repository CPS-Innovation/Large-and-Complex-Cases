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
  type ConnectEgressPayload,
  type ConnectNetAppPayload,
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
  indexingFileTransferPayloadSchema,
  initiateFileTransferPayloadSchema,
  connectEgressPayloadSchema,
  connectNetAppPayloadSchema,
} from "../schemas";
import { type CaseSearchParams } from "../common/types/CaseSearchParams";
import { ApiError } from "../common/errors/ApiError";
import { type TelemetryPayload } from "../TelemetryLogger";

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
  correlationId: string,
): Promise<T> => {
  let parsedJson: unknown;
  try {
    parsedJson = await response.json();
  } catch (error) {
    throw new ApiError(`${error}`, url, response, {
      correlationId,
    });
  }

  const result = schema.safeParse(parsedJson);

  if (!result.success) {
    console.warn(`${contextText} validation failed`, result.error);
    throw new ApiError(`response schema validation failed`, url, response, {
      correlationId,
    });
  }

  return result.data;
};

const fetchOrThrow = async (
  url: string,
  headers: Record<string, string>,
  method: "GET" | "POST" | "PUT" | "DELETE",
  stringifiedPayload?: string,
): Promise<Response> => {
  let response: Response;
  try {
    response = await fetch(url, {
      method,
      credentials: "include",
      headers,
      ...(method === "POST" && stringifiedPayload
        ? { body: stringifiedPayload }
        : {}),
    });
  } catch (networkError) {
    throw new ApiError(
      `${networkError}`,
      url,
      { status: 0, statusText: "Network Error" },
      {
        correlationId: headers[CORRELATION_ID],
      },
    );
  }
  return response;
};

export const getCaseSearchResults = async (
  searchParams: CaseSearchParams,
): Promise<SearchResultData> => {
  const params = new URLSearchParams(searchParams);
  const url = `${GATEWAY_BASE_URL}/api/v1/case-search?${params}`;
  const headers = await buildCommonHeaders();

  const response = await fetchOrThrow(url, headers, "GET");
  if (!response.ok) {
    throw new ApiError(`Searching for cases failed`, url, response, {
      correlationId: headers[CORRELATION_ID],
    });
  }
  const result = await parseAndValidateResponse<SearchResultData>(
    response,
    url,
    searchResultDataSchema,
    "searchResultDataSchema",
    headers[CORRELATION_ID],
  );
  return result;
};

export const getCaseDivisionsOrAreas = async () => {
  const url = `${GATEWAY_BASE_URL}/api/v1/areas`;
  const headers = await buildCommonHeaders();

  const response = await fetchOrThrow(url, headers, "GET");

  if (!response.ok) {
    throw new ApiError(`Getting case areas failed`, url, response, {
      correlationId: headers[CORRELATION_ID],
    });
  }

  const result = await parseAndValidateResponse<CaseDivisionsOrAreaResponse>(
    response,
    url,
    caseDivisionsOrAreaResponseSchema,
    "caseDivisionsOrAreaResponseSchema",
    headers[CORRELATION_ID],
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
  const headers = await buildCommonHeaders();
  const params = new URLSearchParams({
    "workspace-name": workspaceName,
    skip: `${skip}`,
    take: `${take}`,
  });
  const response = await fetchOrThrow(`${url}?${params}`, headers, "GET");
  if (!response.ok) {
    throw new ApiError(
      `Searching for Egress workspaces failed`,
      url,
      response,
      {
        correlationId: headers[CORRELATION_ID],
      },
    );
  }

  const result = await parseAndValidateResponse<EgressSearchResultResponse>(
    response,
    url,
    egressSearchResultResponseSchema,
    "egressSearchResultResponseSchema",
    headers[CORRELATION_ID],
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
  workspaceName,
  caseId,
}: {
  workspaceId: string;
  workspaceName: string;
  caseId: string;
}) => {
  const payload: ConnectEgressPayload = {
    egressWorkspaceId: workspaceId,
    egressWorkspaceName: workspaceName,
    caseId: Number.parseInt(caseId),
  };
  const validatedData = connectEgressPayloadSchema.safeParse(payload);
  if (!validatedData.success) {
    console.warn(
      `Invalid connect Egress workspace request payload: ${validatedData.error}`,
    );
    throw new Error(`Invalid connect Egress workspace request payload`);
  }

  const url = `${GATEWAY_BASE_URL}/api/v1/egress/connections`;
  const headers = await buildCommonHeaders();
  const response = await fetchOrThrow(
    url,
    headers,
    "POST",
    JSON.stringify(payload),
  );

  if (!response.ok) {
    throw new ApiError(`Connecting to Egress workspace failed`, url, response, {
      correlationId: headers[CORRELATION_ID],
    });
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
  const headers = await buildCommonHeaders();
  const params = new URLSearchParams({
    "operation-name": operationName,
    path: folderPath,
    take: `${take}`,
    "continuation-token": continuationToken,
  });
  const response = await fetchOrThrow(`${url}?${params}`, headers, "GET");
  if (!response.ok) {
    throw new ApiError(`getting netapp folders failed`, url, response, {
      correlationId: headers[CORRELATION_ID],
    });
  }
  const result = await parseAndValidateResponse<ConnectNetAppFolderResponse>(
    response,
    url,
    connectNetAppFolderResponseSchema,
    "connectNetAppFolderResponseSchema",
    headers[CORRELATION_ID],
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
  const payload: ConnectNetAppPayload = {
    operationName: operationName,
    folderPath: folderPath,
    caseId: Number.parseInt(caseId),
  };
  const validatedData = connectNetAppPayloadSchema.safeParse(payload);
  if (!validatedData.success) {
    console.warn(
      `Invalid connect Netapp request payload: ${validatedData.error}`,
    );
    throw new Error(`Invalid connect Netapp request payload`);
  }

  const url = `${GATEWAY_BASE_URL}/api/v1/netapp/connections`;
  const headers = await buildCommonHeaders();
  const response = await fetchOrThrow(
    url,
    headers,
    "POST",
    JSON.stringify(payload),
  );

  if (!response.ok) {
    throw new ApiError(`Connecting to NetApp folder failed`, url, response, {
      correlationId: headers[CORRELATION_ID],
    });
  }
  return response;
};

export const disconnectNetAppFolder = async (caseId: number) => {
  const url = `${GATEWAY_BASE_URL}/api/v1/netapp/connections?case-id=${caseId}`;
  const headers = await buildCommonHeaders();
  const response = await fetchOrThrow(url, headers, "DELETE");

  if (!response.ok) {
    throw new ApiError(`Disconnecting NetApp folder failed`, url, response, {
      correlationId: headers[CORRELATION_ID],
    });
  }
  return response;
};

export const getCaseMetaData = async (caseId: string) => {
  const url = `${GATEWAY_BASE_URL}/api/v1/cases/${caseId}`;
  const headers = await buildCommonHeaders();
  const response = await fetchOrThrow(url, headers, "GET");

  if (!response.ok) {
    throw new ApiError(`Getting case metadata failed`, url, response, {
      correlationId: headers[CORRELATION_ID],
    });
  }
  const result = await parseAndValidateResponse<CaseMetaDataResponse>(
    response,
    url,
    caseMetaDataResponseSchema,
    "caseMetaDataResponseSchema",
    headers[CORRELATION_ID],
  );
  return result;
};

export const getEgressFolders = async (
  workspaceId: string,
  folderIdOrPath: string,
  folderParamKey: "folder-id" | "path" = "folder-id",
  skip: number = 0,
  take: number = 50,
  collected: EgressFolderData = [],
): Promise<EgressFolderData> => {
  let folderParam: { "folder-id"?: string } | { path?: string } = {
    "folder-id": folderIdOrPath,
  };

  if (folderParamKey === "path")
    folderParam = { path: folderIdOrPath.replace(/\/$/, "") };
  const params = new URLSearchParams({
    ...folderParam,
    skip: `${skip}`,
    take: `${take}`,
  });
  const url = `${GATEWAY_BASE_URL}/api/v1/egress/workspaces/${workspaceId}/files?${params}`;
  const headers = await buildCommonHeaders();
  const response = await fetchOrThrow(url, headers, "GET");

  if (!response.ok) {
    throw new ApiError(`Getting egress folders failed`, url, response, {
      correlationId: headers[CORRELATION_ID],
    });
  }

  const result = await parseAndValidateResponse<EgressFolderResponse>(
    response,
    url,
    egressFolderResponseSchema,
    "egressFolderResponseSchema",
    headers[CORRELATION_ID],
  );

  const { data, pagination } = result;
  const updated = collected.concat(data);
  if (skip + take >= pagination.totalResults) {
    return updated;
  }
  return getEgressFolders(
    workspaceId,
    folderIdOrPath,
    folderParamKey,
    skip + take,
    take,
    updated,
  );
};

export const getNetAppFolders = async (
  folderPath: string,
  take: number = 50,
  continuationToken = "",
  collectedFolders: NetAppFolder[] = [],
  collectedFiles: NetAppFile[] = [],
): Promise<NetAppFolderDataResponse> => {
  const url = `${GATEWAY_BASE_URL}/api/v1/netapp/files`;
  const headers = await buildCommonHeaders();
  const params = new URLSearchParams({
    path: folderPath,
    take: `${take}`,
    "continuation-token": continuationToken,
  });
  const response = await fetchOrThrow(`${url}?${params}`, headers, "GET");
  if (!response.ok) {
    throw new ApiError(`getting netapp files/folders failed`, url, response, {
      correlationId: headers[CORRELATION_ID],
    });
  }
  const result = await parseAndValidateResponse<NetAppFolderResponse>(
    response,
    url,
    netAppFolderResponseSchema,
    "netAppFolderResponseSchema",
    headers[CORRELATION_ID],
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
  const validatedData = indexingFileTransferPayloadSchema.safeParse(payload);
  if (!validatedData.success) {
    console.warn(
      `Invalid indexing file transfer request payload: ${validatedData.error}`,
    );
    throw new Error(`Invalid indexing file transfer request payload`);
  }
  const url = `${GATEWAY_BASE_URL}/api/v1/filetransfer/files`;
  const headers = await buildCommonHeaders();

  const response = await fetchOrThrow(
    url,
    headers,
    "POST",
    JSON.stringify(payload),
  );

  if (!response.ok) {
    throw new ApiError(`indexing file transfer api failed`, url, response, {
      correlationId: headers[CORRELATION_ID],
    });
  }

  const result = await parseAndValidateResponse<IndexingFileTransferResponse>(
    response,
    url,
    indexingFileTransferResponseSchema,
    "indexingFileTransferResponseSchema",
    headers[CORRELATION_ID],
  );
  return result;
};

export const initiateFileTransfer = async (
  payload: InitiateFileTransferPayload,
) => {
  const validatedData = initiateFileTransferPayloadSchema.safeParse(payload);
  if (!validatedData.success) {
    console.warn(
      `Invalid initiate file transfer request payload: ${validatedData.error}`,
    );
    throw new Error(`Invalid initiate file transfer request payload`);
  }

  const url = `${GATEWAY_BASE_URL}/api/v1/filetransfer/initiate`;
  const headers = await buildCommonHeaders();

  const response = await fetchOrThrow(
    url,
    headers,
    "POST",
    JSON.stringify(payload),
  );

  if (!response.ok) {
    throw new ApiError(`initiate file transfer failed`, url, response, {
      correlationId: headers[CORRELATION_ID],
    });
  }

  const result = await parseAndValidateResponse<InitiateFileTransferResponse>(
    response,
    url,
    initiateFileTransferResponseSchema,
    "initiateFileTransferResponseSchema",
    headers[CORRELATION_ID],
  );
  return result;
};

export const getTransferStatus = async (
  transferId: string,
  etag?: string,
): Promise<{
  data: TransferStatusResponse | null;
  etag: string | null;
  correlationId: string;
}> => {
  const url = `${GATEWAY_BASE_URL}/api/v1/filetransfer/${transferId}/status`;
  const headers = await buildCommonHeaders();
  let response: Response;
  try {
    response = await fetch(url, {
      method: "GET",
      credentials: "include",
      headers: {
        ...headers,
        ...(etag ? { "If-None-Match": etag } : {}),
      },
    });
  } catch (networkError) {
    throw new ApiError(
      `${networkError}`,
      url,
      { status: 0, statusText: "Network Error" },
      {
        correlationId: headers[CORRELATION_ID],
      },
    );
  }

  if (response.status === 304) {
    return {
      data: null,
      etag: etag ?? null,
      correlationId: headers[CORRELATION_ID],
    };
  }

  if (!response.ok) {
    throw new ApiError(`Getting case transfer status failed`, url, response, {
      correlationId: headers[CORRELATION_ID],
    });
  }

  const result = await parseAndValidateResponse<TransferStatusResponse>(
    response,
    url,
    transferStatusResponseSchema,
    "transferStatusResponseSchema",
    headers[CORRELATION_ID],
  );
  return {
    data: result,
    etag: response.headers.get("ETag"),
    correlationId: headers[CORRELATION_ID],
  };
};

export const handleFileTransferClear = async (transferId: string) => {
  const url = `${GATEWAY_BASE_URL}/api/v1/filetransfer/${transferId}/clear`;
  const headers = await buildCommonHeaders();
  const response = await fetchOrThrow(url, headers, "POST");

  if (!response.ok) {
    throw new ApiError(`clear file transfer api failed`, url, response, {
      correlationId: headers[CORRELATION_ID],
    });
  }
};

export const getActivityLog = async (caseId: string) => {
  const params = new URLSearchParams({
    "case-id": caseId,
  });
  const url = `${GATEWAY_BASE_URL}/api/v1/activity/logs?${params}`;
  const headers = await buildCommonHeaders();
  const response = await fetchOrThrow(url, headers, "GET");

  if (!response.ok) {
    throw new ApiError(`Getting case activity log failed`, url, response, {
      correlationId: headers[CORRELATION_ID],
    });
  }
  const result = await parseAndValidateResponse<ActivityLogResponse>(
    response,
    url,
    activityLogResponseSchema,
    "activityLogResponseSchema",
    headers[CORRELATION_ID],
  );
  return result;
};

export const downloadActivityLog = async (activityId: string) => {
  const url = `${GATEWAY_BASE_URL}/api/v1/activity/${activityId}/logs/download`;
  const headers = await buildCommonHeaders();
  const response = await fetchOrThrow(url, headers, "GET");

  if (!response.ok) {
    throw new ApiError(`Downloading activity log failed`, url, response, {
      correlationId: headers[CORRELATION_ID],
    });
  }
  return response;
};

export const logTelemetryEvent = async (payload: TelemetryPayload) => {
  try {
    const url = `${GATEWAY_BASE_URL}/api/v1/telemetry`;
    const headers = await buildCommonHeaders();
    const response = await fetch(url, {
      method: "POST",
      credentials: "include",
      headers,
      body: JSON.stringify(payload),
    });
    if (!response.ok) {
      console.warn(
        `Logging telemetry event failed with status: ${response.status}`,
      );
    }
  } catch (error) {
    // Fail silently to ensure UI flows remain unblocked
    console.warn(
      "Logging telemetry event failed due to network or auth error:",
      error,
    );
  }
};
