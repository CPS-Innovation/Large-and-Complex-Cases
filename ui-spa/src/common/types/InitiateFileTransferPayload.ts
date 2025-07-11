export type InitiateFileTransferPayload =
  | EgreessToNetAppTransferPayload
  | NetAppToEgressTransferPayload;

export type EgreessToNetAppTransferPayload = {
  workspaceId: string;
  caseId: number;
  transferType: "Copy" | "Move";
  transferDirection: "EgressToNetApp";
  sourcePaths: EgressTranferPayloadSourcePath[];
  destinationPath: string;
};

export type NetAppToEgressTransferPayload = {
  caseId: number;
  transferType: "Copy";
  transferDirection: "NetAppToEgress";
  sourcePaths: NetAppTranferPayloadSourcePath[];
  destinationPath: string;
};

export type EgressTranferPayloadSourcePath = {
  fileId?: string;
  path: string;
  modifiedPath?: string;
};

export type NetAppTranferPayloadSourcePath = {
  path: string;
  modifiedPath?: string;
  relativePath?: string;
};
