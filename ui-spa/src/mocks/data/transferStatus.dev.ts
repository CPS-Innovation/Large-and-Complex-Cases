import { type TransferStatusResponse } from "../../schemas";
export const egressToNetAppTransferStatusDev: TransferStatusResponse = {
  id: "00000000-0000-4000-8000-000000000001",
  status: "PartiallyCompleted",
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
  successItems: [
    {
      path: "folder1/folder2/file1.txt",
    },
    {
      path: "folder1/folder2/file2.txt",
    },
    {
      path: "folder1/folder3/file3.txt",
    },
  ],
  destinationFolderName: "demo-statements",
  destinationPath: "netapp/folder-1-1/",
  destinationId: "",
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
