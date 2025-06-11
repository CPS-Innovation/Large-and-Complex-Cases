export type InitiateFileTransferPayload = {
  isRetry: boolean;
  caseId: string;
  transferType: "COPY" | "MOVE";
  direction: "EgressToNetApp" | "NetAppToEgress";
  sourcePaths: [
    {
      id: string;
      path: string;
      modifiedPath: string;
      overwritePolicy: "OVERWRITE";
    },
    {
      id: string;
      path: string;
    },
  ];
  destinationPath: string;
};
