import { IndexingFileTransferResponse } from "../../common/types/IndexingFileTransferResponse";

export const egressToNetAppIndexingTransferDev: IndexingFileTransferResponse = {
  caseId: 12,
  isInvalid: false,
  destinationPath: "abc/",
  validationErrors: [],
  sourceRootFolderPath: "egress/",
  transferDirection: "EgressToNetApp",
  files: [
    {
      id: "id_1",
      sourcePath: "egress/folder1/file1.pdf",
    },
    { id: "id_2", sourcePath: "egress/folder1/folder2/file2.pdf" },
  ],
};

export const netAppToEgressIndexingTransferDev: IndexingFileTransferResponse = {
  caseId: 12,
  isInvalid: false,
  destinationPath: "abc/",
  validationErrors: [],
  sourceRootFolderPath: "netapp/",
  transferDirection: "NetAppToEgress",
  files: [
    { sourcePath: "netapp/folder1/file1.pdf" },
    { sourcePath: "netapp/folder1/folder2/file2.pdf" },
  ],
};

export const egressToNetAppIndexingErrorDev: IndexingFileTransferResponse = {
  caseId: 12,
  isInvalid: true,
  sourceRootFolderPath: "egress/",
  transferDirection: "EgressToNetApp",
  destinationPath:
    "egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/folder1/egress/destination/",
  validationErrors: [
    {
      id: "id_3",
      sourcePath:
        "egress/folder3/file3qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswweee.pdf",
    },
    {
      id: "id_4",
      sourcePath:
        "egress/folder3/file4qeeweweweweewwwweeewwwwwwwwwwwwwwwwwwwwwwwssssssswwee.pdf",
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
