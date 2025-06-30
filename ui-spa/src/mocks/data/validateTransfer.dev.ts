import { IndexingFileTransferResponse } from "../../common/types/IndexingFileTransferResponse";

export const egressToNetAppIndexingTransferDev: IndexingFileTransferResponse = {
  caseId: 12,
  isInvalid: false,
  destinationPath: "abc/",
  validationErrors: [],
  files: [
    {
      id: "id_1",
      sourcePath: "egress/folder1",
    },
    { id: "id_2", sourcePath: "egress/folder1/folder2" },
  ],
};

export const netAppToEgressIndexingTransferDev: IndexingFileTransferResponse = {
  caseId: 12,
  isInvalid: false,
  destinationPath: "abc/",
  validationErrors: [],
  files: [
    { sourcePath: "netapp/folder1" },
    { sourcePath: "netapp/folder1/folder2" },
  ],
};

export const egressToNetAppIndexingErrorDev: IndexingFileTransferResponse = {
  caseId: 12,
  isInvalid: true,
  destinationPath:
    "egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1",
  validationErrors: [
    {
      id: "id_3",
      sourcePath:
        "egress/folder3/file3qeeweweweweewweewwewewewewewewweewerwrrwwrwrrrrrrwrwrwrwweewweeewweweeweweweew.pdf",
      errorType: "",
    },
    { id: "id_4", sourcePath: "egress/folder3/file4.pdf", errorType: "" },
    { id: "id_5", sourcePath: "egress/folder4/file4.pdf", errorType: "" },
    {
      id: "id_6",
      sourcePath: "egress/folder3/folder1/file4.pdf",
      errorType: "",
    },
  ],
  files: [
    { id: "id_1", sourcePath: "egress/folder1/file1.pdf" },
    { id: "id_2", sourcePath: "egress/folder1/file2.pdf" },
  ],
};
