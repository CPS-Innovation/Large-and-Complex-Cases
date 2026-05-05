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

export type BatchCopyItem = {
  sourcePath: string;
  destinationPath?: string;
  outcome: string;
  type: string;
};

export type BatchCopyDetails = {
  items: BatchCopyItem[];
};

export function isTransferDetails(
  details: TransferDetails | BatchDeleteDetails | BatchCopyDetails | null,
): details is TransferDetails {
  return details !== null && "destinationPath" in details;
}

export function isBatchCopyDetails(
  details: TransferDetails | BatchDeleteDetails | BatchCopyDetails | null,
): details is BatchCopyDetails {
  return (
    details !== null &&
    "items" in details &&
    Array.isArray((details as BatchCopyDetails).items) &&
    (details as BatchCopyDetails).items.length > 0 &&
    "type" in (details as BatchCopyDetails).items[0]
  );
}

export function isBatchDeleteDetails(
  details: TransferDetails | BatchDeleteDetails | BatchCopyDetails | null,
): details is BatchDeleteDetails {
  return details !== null && "items" in details && !isBatchCopyDetails(details);
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
  details: TransferDetails | BatchDeleteDetails | BatchCopyDetails | null;
};

export type ActivityLogResponse = {
  data: ActivityItem[];
};
