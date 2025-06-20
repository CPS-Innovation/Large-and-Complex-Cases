export type ValidateFileTransferPayload = {
  caseId: string;
  transferType: "COPY" | "MOVE";
  direction: "EgressToNetApp" | "NetAppToEgress";
  sourcePaths: {
    id?: string;
    path: string;
  }[];
  destinationBasePath: string;
};
