import { TransferStatusResponse } from "../../common/types/TransferStatusResponse";
export const egressToNetAppTransferStatusPlaywright: TransferStatusResponse = {
  status: "Completed",
  transferType: "Copy",
  direction: "EgressToNetApp",
  completedAt: null,
  failedItems: [],
  userName: "dev_user@example.org",
  totalFiles: 30,
  processedFiles: 30,
  successfulFiles: 30,
  failedFiles: 0,
};

export const netAppToEgressTransferStatusPlaywright: TransferStatusResponse = {
  status: "Completed",
  transferType: "Copy",
  direction: "NetAppToEgress",
  completedAt: null,
  failedItems: [],
  userName: "dev_user@example.org",
  totalFiles: 30,
  processedFiles: 30,
  successfulFiles: 30,
  failedFiles: 0,
};
