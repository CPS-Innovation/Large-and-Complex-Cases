import { type TransferStatusResponse } from "../../schemas";
export const egressToNetAppTransferStatusDev: TransferStatusResponse = {
  id: "00000000-0000-4000-8000-000000000001",
  status: "Completed",
  transferType: "Copy",
  direction: "EgressToNetApp",
  startedAt: null,
  completedAt: null,
  failedItems: [
    {
      sourcePath: "folder1/folder2/file1.txt",
      errorCode: "FileExists",
    },
    {
      sourcePath: "folder1/folder2/file2.txt",
      errorCode: "FileExists",
    },
    {
      sourcePath: "folder1/folder3/file2.txt",
      errorCode: "FileExists",
    },
    {
      sourcePath: "folder1/folder2/file2.txt",
      errorCode: "GeneralError",
    },
  ],
  userName: "dev_user@example.org",
  totalFiles: 30,
  processedFiles: 28,
  successfulFiles: 28,
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
  destinationPath: "egress/folder-1-1/",
};

export const netAppToEgressTransferStatusDev: TransferStatusResponse = {
  id: "00000000-0000-4000-8000-000000000002",
  status: "Completed",
  transferType: "Copy",
  direction: "NetAppToEgress",
  startedAt: null,
  completedAt: null,
  failedItems: [],
  successfulItems: [],
  destinationPath: "netapp/folder1/folder2/",
  userName: "dev_user@example.org",
  totalFiles: 30,
  processedFiles: 30,
  successfulFiles: 30,
  failedFiles: 0,
};
