import { type TransferStatusResponse } from "../../schemas";
export const egressToNetAppTransferStatusDev: TransferStatusResponse = {
  id: "00000000-0000-4000-8000-000000000001",
  status: "Completed",
  transferType: "Copy",
  direction: "EgressToNetApp",
  startedAt: null,
  completedAt: null,
  failedItems: [],
  userName: "dev_user@example.org",
  totalFiles: 30,
  processedFiles: 30,
  successfulFiles: 30,
  failedFiles: 0,
};

export const netAppToEgressTransferStatusDev: TransferStatusResponse = {
  id: "00000000-0000-4000-8000-000000000002",
  status: "Completed",
  transferType: "Copy",
  direction: "NetAppToEgress",
  startedAt: null,
  completedAt: null,
  failedItems: [],
  userName: "dev_user@example.org",
  totalFiles: 30,
  processedFiles: 30,
  successfulFiles: 30,
  failedFiles: 0,
};

export const netAppToNetAppTransferStatusDev: TransferStatusResponse = {
  id: "00000000-0000-4000-8000-000000000003",
  status: "Completed",
  transferType: "Copy",
  direction: "NetAppToNetApp",
  startedAt: null,
  completedAt: null,
  failedItems: [],
  userName: "dev_user@example.org",
  totalFiles: 5,
  processedFiles: 5,
  successfulFiles: 5,
  failedFiles: 0,
};
