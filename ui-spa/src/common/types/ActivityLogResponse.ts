export type ActivityItem = {
  id: string;
  actionType:
    | "TRANSFER_INITIATED"
    | "TRANSFER_COMPLETED"
    | "TRANSFER_FAILED"
    | "CONNECTION_TO_EGRESS"
    | "CONNECTION_TO_NETAPP";

  timestamp: string;
  userName: string;
  caseId: string;
  description: string;
  resourceType?: "FileTransfer";
  resourceName?: string;
  details: {
    transferId: string;
    sourcePath: string;
    destinationPath: string;
    transferType: "Move" | "Copy";
    errorFileCount: number;
    transferedFileCount: number;
    totalFiles: number;
    files: {
      path: string;
    }[];
    errors: {
      path: string;
    }[];
    deletionErrors: { path: string }[];
  } | null;
};

export type ActivityLogResponse = {
  data: ActivityItem[];
};
