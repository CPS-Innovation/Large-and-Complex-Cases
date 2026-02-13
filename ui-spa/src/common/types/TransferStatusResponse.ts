export type TransferStatusResponse = {
  status:
    | "Initiated"
    | "InProgress"
    | "Completed"
    | "PartiallyCompleted"
    | "Failed";
  transferType: "Copy" | "Move";
  direction: "EgressToNetApp" | "NetAppToEgress";
  completedAt: null | string;
  failedItems: TransferFailedItem[];
  userName: string;
  totalFiles: number;
  processedFiles: number;
  successfulFiles: number;
  failedFiles: number;
};

export type TransferFailedItem = {
  sourcePath: string;
  errorCode: "FileExists" | "GeneralError" | "IntegrityVerificationFailed";
};
