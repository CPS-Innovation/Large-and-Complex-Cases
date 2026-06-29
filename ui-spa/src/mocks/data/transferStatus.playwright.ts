import { type TransferStatusResponse } from "../../schemas";
export const egressToNetAppTransferStatusPlaywright: TransferStatusResponse = {
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
  successfulItems: [],
  destinationPath: "egress/folder-1-1/",
};

export const netAppToEgressTransferStatusPlaywright: TransferStatusResponse = {
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
  successfulItems: [],
  destinationPath: "netapp/folder-1-1/",
};
