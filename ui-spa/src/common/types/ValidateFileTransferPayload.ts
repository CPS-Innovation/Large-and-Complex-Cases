export type ValidateFileTransferPayload = {
  caseId: number;
  // transferType: "COPY" | "MOVE";
  transferDirection: "EgressToNetApp" | "NetAppToEgress";
  sourcePaths: {
    fileId?: string;
    path: string;
    isFolder?: boolean;
  }[];
  destinationPath: string;
  workspaceId?: string;
};
