export type ActivityItem = {
  id: string;
  actionType:
    | "TRANSFER_INITIATED"
    | "TRANSFER_COMPLETED"
    | "TRANSFER_FAILED"
    | "CONNECTION_TO_EGRESS"
    | "CONNECTION_TO_NETAPP";

  timestamp: string;
  userId: string;
  userName: string;
  caseId: string;
  description: string;
  resourceType?: "FileTransfer";
  resourceName?: string;
  details: {
    transferId: string;
    sourceSystem: string;
    destinationSystem: string;
    fileCount: number;
    sourcePath: string;
    destinationPath: string;
    files: {
      sourcePath: string;
    }[];
    errors: {
      sourcePath: string;
    }[];
  } | null;
};

export type ActivityLogResponse = {
  items: ActivityItem[];
};
