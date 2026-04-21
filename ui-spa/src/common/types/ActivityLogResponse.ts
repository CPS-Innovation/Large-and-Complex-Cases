export type TransferDetails = {
  transferId: string;
  sourcePath: string;
  destinationPath: string;
  transferType: "Move" | "Copy";
  errorFileCount: number;
  transferedFileCount: number;
  totalFiles: number;
  files: { path: string }[];
  errors: { path: string }[];
  deletionErrors: { path: string }[];
};

export type BatchDeleteItem = {
  sourcePath: string;
  outcome: string;
  error: string | null;
  keysDeleted: number | null;
};

export type BatchDeleteDetails = {
  items: BatchDeleteItem[];
};

export function isTransferDetails(
  details: TransferDetails | BatchDeleteDetails | null,
): details is TransferDetails {
  return details !== null && "destinationPath" in details;
}

export function isBatchDeleteDetails(
  details: TransferDetails | BatchDeleteDetails | null,
): details is BatchDeleteDetails {
  return details !== null && "items" in details;
}

export type ActivityItem = {
  id: string;
  actionType: string;
  timestamp: string;
  userName: string;
  caseId: string;
  description: string;
  resourceType?: string;
  resourceName?: string;
  details: TransferDetails | BatchDeleteDetails | null;
};

export type ActivityLogResponse = {
  data: ActivityItem[];
};
