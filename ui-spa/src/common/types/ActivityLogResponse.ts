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
    TransferId: string;
    SourceSystem: string;
    DestinationSystem: string;
    FileCount: number;
    SourcePath: string;
    DestinationPath: string;
    Files: {
      Path: string;
    }[];
    Errors: {
      Path: string;
    }[];
  } | null;
};

export type ActivityLogResponse =  ActivityItem[];
