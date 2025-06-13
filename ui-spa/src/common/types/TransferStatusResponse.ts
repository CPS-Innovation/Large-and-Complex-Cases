export type TransferStatusResponse = {
  overallStatus:
    | "INITIATED"
    | "IN_PROGRESS"
    | "COMPLETED"
    | "PARTIALLY_COMPLETED"
    | "FAILED";
  transferType: "COPY" | "MOVE";
  direction: "EgressToNetApp" | "NetAppToEgress";
  completedAt: null | string;
  failedFiles: TransferFailedItem[];
};

export type TransferFailedItem = {
  sourcePath: string;
  transferErrorCode: "DUPLICATE" | "NETWORK_ERROR" | "FILE_IN_USE";
};
