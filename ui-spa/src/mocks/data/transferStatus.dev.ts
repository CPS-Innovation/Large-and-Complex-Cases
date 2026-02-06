import { TransferStatusResponse } from "../../common/types/TransferStatusResponse";
export const egressToNetAppTransferStatusDev: TransferStatusResponse = {
  status: "PartiallyCompleted",
  transferType: "Copy",
  direction: "EgressToNetApp",
  completedAt: null,
  failedItems: [
    {
      sourcePath: "Demo/450mb file test.txt",
      errorCode: "FileExists",
    },
    {
      sourcePath: "Demo/RB_TEST.docx",
      errorCode: "FileExists",
    },
    {
      sourcePath:
        "Demo/test1/test2/test3/test4/Demo/test1/test2/test3/test4/Demo/test1/test2/test3/test4/Demo/test1/test2/test3/test4/Demo/test1/test2/test3/test4/Demo/test1/test2/test3/test4/test2GBFile.txt",
      errorCode: "FileExists",
    },
    {
      sourcePath:
        "Demo/test1/test2/test3/test4/Demo/test1/test2/test3/test4/Demo/test1/test2/test3/test4/Demo/test1/test2/test3/test4/Demo/test1/test2/test3/test4/Demo/test1/test2/test3/test4Demo/activity-log-01980874-7809-7b6d-a16c-ba10016e9ee1-files.csv",
      errorCode: "FileExists",
    },
    {
      sourcePath: "Demo/Free_Test_Data_10.5MB_PDF.pdf",
      errorCode: "GeneralError",
    },
    {
      sourcePath: "Demo/450mb file test.txt",
      errorCode: "GeneralError",
    },
    {
      sourcePath: "Demo/RB_TEST.docx",
      errorCode: "GeneralError",
    },
    {
      sourcePath: "Demo/test2GBFile.txt",
      errorCode: "GeneralError",
    },
    {
      sourcePath:
        "Demo/activity-log-01980874-7809-7b6d-a16c-ba10016e9ee1-files.csv",
      errorCode: "GeneralError",
    },
  ],
  userName: "dev_user@example.org",
};

export const netAppToEgressTransferStatusDev: TransferStatusResponse = {
  status: "Completed",
  transferType: "Copy",
  direction: "NetAppToEgress",
  completedAt: null,
  failedItems: [],
  userName: "dev_user@example.org",
};
