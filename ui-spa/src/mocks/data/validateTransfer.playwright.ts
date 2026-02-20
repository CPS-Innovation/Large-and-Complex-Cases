import { IndexingFileTransferResponse } from "../../common/types/IndexingFileTransferResponse";

export const egressToNetAppIndexingTransferPlaywright: IndexingFileTransferResponse =
  {
    caseId: 12,
    isInvalid: false,
    destinationPath: "abc/",
    sourceRootFolderPath: "egress/",
    transferDirection: "EgressToNetApp",
    validationErrors: [],
    files: [
      { id: "id_1", sourcePath: "egress/folder1/file1.pdf" },
      { id: "id_2", sourcePath: "egress/folder1/folder2/file2.pdf" },
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
      { sourcePath: "netapp/folder1/file1.pdf" },
      { sourcePath: "netapp/folder1/folder2/file2.pdf" },
    ],
  };
