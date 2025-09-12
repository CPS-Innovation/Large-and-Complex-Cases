export type InitiateFileTransferPayload =
  | EgreessToNetAppTransferPayload
  | NetAppToEgressTransferPayload;

export type EgreessToNetAppTransferPayload = {
  workspaceId: string;
  caseId: number;
  transferType: "Copy" | "Move";
  transferDirection: "EgressToNetApp";
  sourcePaths: EgressTransferPayloadSourcePath[];
  destinationPath: string;
};

export type NetAppToEgressTransferPayload = {
  caseId: number;
  transferType: "Copy";
  transferDirection: "NetAppToEgress";
  sourcePaths: NetAppTransferPayloadSourcePath[];
  sourceRootFolderPath: string;
  destinationPath: string;
};

export type EgressTransferPayloadSourcePath = {
  fileId?: string;
  path: string;
  modifiedPath?: string;
  fullFilePath?: string;
};

export type NetAppTransferPayloadSourcePath = {
  path: string;
  modifiedPath?: string;
  relativePath?: string;
};
