import { IndexingFileTransferResponse } from "../../common/types/IndexingFileTransferResponse";

export const egressToNetAppIndexingTransferPlaywright: IndexingFileTransferResponse =
  {
    caseId: 12,
    isInvalid: false,
    destinationPath: "abc/",
    sourceRootFolderPath: "egress/",
    validationErrors: [],
    files: [
      { id: "id_1", sourcePath: "egress/folder1" },
      { id: "id_2", sourcePath: "egress/folder1/folder2" },
    ],
  };

export const netAppToEgressIndexingTransferPlaywright: IndexingFileTransferResponse =
  {
    caseId: 12,
    isInvalid: false,
    destinationPath: "abc/",
    sourceRootFolderPath: "netapp/",
    validationErrors: [],
    files: [
      { sourcePath: "netapp/folder1" },
      { sourcePath: "netapp/folder1/folder2" },
    ],
  };
