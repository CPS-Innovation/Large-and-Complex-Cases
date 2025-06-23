import { ValidateFileTransferResponse } from "../../common/types/ValidateFileTransferResponse";

export const egressToNetAppValidateTransferPlaywright: ValidateFileTransferResponse =
  {
    caseId: "12",
    isValid: true,
    destinationBasePath: "abc/",
    errors: [],
    discoveredFiles: [
      { id: "id_1", sourcePath: "egress/folder1" },
      { id: "id_2", sourcePath: "egress/folder1/folder2" },
    ],
  };

export const netAppToEgressValidateTransferPlaywright: ValidateFileTransferResponse =
  {
    caseId: "12",
    isValid: true,
    destinationBasePath: "abc/",
    errors: [],
    discoveredFiles: [
      { sourcePath: "netapp/folder1" },
      { sourcePath: "netapp/folder1/folder2" },
    ],
  };
