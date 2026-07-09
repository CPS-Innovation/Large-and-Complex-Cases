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
  successfulItems: [
    {
      sourcePath: "folder1/folder2/file1.txt",
    },
    {
      sourcePath: "folder1/folder2/file2.txt",
    },
    {
      sourcePath: "folder1/folder3/file3.txt",
    },
  ],
  destinationPath: "folder-1-0/folder-2-0/",
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
