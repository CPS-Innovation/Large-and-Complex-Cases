import { getTransferActivityStatusTagData } from "./getTransferActivityStatusTagData";
import { ActivityItem } from "../types/ActivityLogResponse";

describe("getTransferActivityStatusTagData", () => {
  const activity: ActivityItem = {
    id: "4",
    actionType: "TRANSFER_COMPLETED",
    timestamp: "2024-01-18T12:46:10.865517Z",
    userName: "dwight_schrute@cps.gov.uk",
    caseId: "case_1",
    description: "Documents/folders copied from egress to shared drive",
    details: {
      transferId: "transfer-1",
      totalFiles: 6,
      errorFileCount: 2,
      transferedFileCount: 4,
      sourcePath: "egress/folder1",
      destinationPath: "netapp/folder2",
      files: [
        {
          path: "egress/folder1/folder3/file1.pdf",
        },
        {
          path: "egress/folder1/file2.pdf",
        },
        {
          path: "egress/folder1/folder4/folder22/file3.pdf",
        },
        {
          path: "egress/folder1/file4.pdf",
        },
      ],
      errors: [
        {
          path: "egress/folder1/file5.pdf",
        },
        {
          path: "egress/folder1/folder2/file6.pdf",
        },
      ],
      deletionErrors: [],
    },
  };

  it("Should return null if the actviity actionType is neither TRANSFER_COMPLETED nor TRANSFER_FAILED", () => {
    expect(
      getTransferActivityStatusTagData({
        ...activity,
        actionType: "TRANSFER_INITIATED",
      }),
    ).toEqual(null);
    expect(
      getTransferActivityStatusTagData({
        ...activity,
        actionType: "CONNECTION_TO_EGRESS",
      }),
    ).toEqual(null);
    expect(
      getTransferActivityStatusTagData({
        ...activity,
        actionType: "CONNECTION_TO_NETAPP",
      }),
    ).toEqual(null);
  });
  it("Should return null if the actviity details is null", () => {
    expect(
      getTransferActivityStatusTagData({
        ...activity,
        details: null,
      }),
    ).toEqual(null);
  });
  it("Should return failed tag name and red color when the transferedFileCount is zero", () => {
    expect(
      getTransferActivityStatusTagData({
        ...activity,
        details: {
          ...activity.details!,
          transferedFileCount: 0,
        },
      }),
    ).toEqual({ color: "red", name: "Failed" });
  });

  it("Should return Completed tag name and green color when the all of the files are fully transfered and there are no errors", () => {
    expect(
      getTransferActivityStatusTagData({
        ...activity,
        details: {
          ...activity.details!,
          transferedFileCount: 6,
          errorFileCount: 0,
        },
      }),
    ).toEqual({ color: "green", name: "Completed" });
  });
  it("Should return Completed with errors tag name and green yellow when the there are non zero transferred file count and non zero error file count ", () => {
    expect(
      getTransferActivityStatusTagData({
        ...activity,
        details: {
          ...activity.details!,
          transferedFileCount: 4,
          errorFileCount: 2,
        },
      }),
    ).toEqual({ color: "yellow", name: "Completed with errors" });
  });
});
