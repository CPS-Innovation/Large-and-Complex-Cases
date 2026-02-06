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
};

export type TransferFailedItem = {
  sourcePath: string;
  errorCode: "FileExists" | "GeneralError" | "IntegrityVerificationFailed";
};
