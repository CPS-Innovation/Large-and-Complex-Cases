import { type IndexingFileTransferResponse } from "../../schemas";

export const egressToNetAppIndexingTransferPlaywright: IndexingFileTransferResponse =
  {
    caseId: 12,
    isInvalid: false,
    destinationPath: "abc/",
    sourceRootFolderPath: "egress/",
    transferDirection: "EgressToNetApp",
    validationErrors: [],
    files: [
      {
        id: "id_1",
        sourcePath: "file1.pdf",
        relativePath: null,
        fullFilePath: "egress/folder1/file1.pdf",
      },
      {
        id: "id_2",
        sourcePath: "file2.pdf",
        relativePath: null,
        fullFilePath: "egress/folder1/folder2/file2.pdf",
      },
    ],
  };

export const netAppToEgressIndexingTransferPlaywright: IndexingFileTransferResponse =
  {
    caseId: 12,
    isInvalid: false,
    destinationPath: "abc/",
    sourceRootFolderPath: "netapp/",
    transferDirection: "NetAppToEgress",
    validationErrors: [],
    files: [
      {
        sourcePath: "netapp/folder1/file1.pdf",
        relativePath: "file1.pdf",
        id: null,
        fullFilePath: null,
      },
      {
        sourcePath: "netapp/folder1/folder2/file2.pdf",
        relativePath: "file2.pdf",
        id: null,
        fullFilePath: null,
      },
    ],
  };
