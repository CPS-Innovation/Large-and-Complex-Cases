export type InitiateFileTransferPayload =
  | EgreessToNetAppTransferPayload
  | NetAppToEgressTransferPayload;

export type EgreessToNetAppTransferPayload = {
  isRetry: boolean;
  caseId: string;
  transferType: "COPY" | "MOVE";
  direction: "EgressToNetApp";
  sourcePaths: EgressTranferPayloadSourcPath[];
  destinationPath: string;
};

export type NetAppToEgressTransferPayload = {
  isRetry: boolean;
  caseId: string;
  transferType: "COPY";
  direction: "NetAppToEgress";
  sourcePaths: NetAppTranferPayloadSourcPath[];
  destinationPath: string;
};

export type EgressTranferPayloadSourcPath = {
  id: string;
  path: string;
  modifiedPath?: string;
};

export type NetAppTranferPayloadSourcPath = {
  path: string;
};
