import { TransferStatusResponse } from "../../common/types/TransferStatusResponse";
export const egressToNetAppTransferStatusDev: TransferStatusResponse = {
  status: "Completed",
  transferType: "Copy",
  direction: "EgressToNetApp",
  completedAt: null,
  failedItems: [],
  userName: "dev_user@example.org",
  totalFiles: 30,
  processedFiles: 30,
};

export const netAppToEgressTransferStatusDev: TransferStatusResponse = {
  status: "Completed",
  transferType: "Copy",
  direction: "NetAppToEgress",
  completedAt: null,
  failedItems: [],
  userName: "dev_user@example.org",
  totalFiles: 30,
  processedFiles: 30,
};
