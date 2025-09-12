import { IndexingFileTransferResponse } from "../../common/types/IndexingFileTransferResponse";

export const egressToNetAppIndexingTransferDev: IndexingFileTransferResponse = {
  caseId: 12,
  isInvalid: false,
  destinationPath: "abc/",
  validationErrors: [],
  sourceRootFolderPath: "egress/",
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
  sourceRootFolderPath: "netapp/",
  files: [
    { sourcePath: "netapp/folder1" },
    { sourcePath: "netapp/folder1/folder2" },
  ],
};

export const egressToNetAppIndexingErrorDev: IndexingFileTransferResponse = {
  caseId: 12,
  isInvalid: true,
  sourceRootFolderPath: "egress/",
  destinationPath:
    "egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/destination",
  validationErrors: [
    {
      id: "id_3",
      sourcePath:
        "egress/folder3/file3qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf",
    },
    {
      id: "id_5",
      sourcePath:
        "egress/folder4/folder5/file5qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssweesss.pdf",
    },
  ],
  files: [
    { id: "id_1", sourcePath: "egress/folder1/file1.pdf" },
    { id: "id_2", sourcePath: "egress/folder1/file2.pdf" },
  ],
};
