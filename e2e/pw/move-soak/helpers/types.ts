export interface MoveSoakScenario {
  name: string;
  specs: FileBatchSpec[];
  timeout: number;
  concurrency?: number;
  injectFailure?: boolean;
}

export type FileBatchSpec = {
  fileSizeMb: number;
  fileCount: number;
};

export type HarnessConfig = {
  apiBaseUrl: string;
  tenantId: string;
  apiClientId: string;
  aadUsername: string,
  aadPassword: string
  workspaceId: string;
  egressSourceFolder: string;
  egressBaseUrl: string;
  serviceAccountAuth: string;
  netappFolderPath: string;
  caseId: number;
};

export type TransferSourcePath = {
  fileId?: string;
  path: string;
  fullFilePath?: string;
};

export type InitiateTransferPayload = {
  workspaceId: string;
  caseId: number;
  transferType: "Move" | "Copy";
  transferDirection: "EgressToNetApp";
  sourcePaths: TransferSourcePath[];
  sourceRootFolderPath: string;
  destinationPath: string;
};

export type InitiateTransferResponse = {
  id: string;
  status: string;
  createdAt: string;
};

export type TransferStatusCheckResponse =  {
  id: string;
  status: "Completed" | "InProgress" | "Initiated" | "PartiallyCompleted" | "Failed";
  failedItems: string[];
  successfulItems: string[];
  totalFiles: number;
  processedFiles: number;
  successfulFiles: number;
  failedFiles: number;
};