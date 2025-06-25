export type IndexingFileTransferPayload = {
  caseId: number;
  transferDirection: "EgressToNetApp" | "NetAppToEgress";
  sourcePaths: {
    fileId?: string;
    path: string;
    isFolder?: boolean;
  }[];
  destinationPath: string;
  workspaceId?: string;
};
