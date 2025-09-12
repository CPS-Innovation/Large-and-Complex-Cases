export type IndexingFileTransferPayload = {
  caseId: number;
  transferDirection: "EgressToNetApp" | "NetAppToEgress";
  transferType: "Move" | "Copy";
  sourcePaths: {
    fileId?: string;
    path: string;
    isFolder?: boolean;
  }[];
  destinationPath: string;
  sourceRootFolderPath: string;
  workspaceId?: string;
};
