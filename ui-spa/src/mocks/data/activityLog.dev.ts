import { ActivityLogResponse } from "../../common/types/ActivityLogResponse";
export const activityLogDev: ActivityLogResponse = {
  data: [
    // {
    //   id: "6",
    //   actionType: "TRANSFER_FAILED",
    //   timestamp: "2024-01-15T11:45:10.865517Z",
    //   userId: "dwight_schrute@cps.gov.uk",
    //   userName: "David",
    //   caseId: "case_1",
    //   description: "Document/folders copied from egress to shared drive",
    //   details: {
    //     transferId: "transfer-1",
    //     sourceSystem: "egress",
    //     destinationSystem: "netapp",
    //     fileCount: 2,
    //     sourcePath: "egress",
    //     destinationPath: "netapp/folder2",
    //     files: [
    //       {
    //         path: "egress/folder1/file1.pdf",
    //       },
    //       {
    //         path: "egress/folder1/folder2/folder3/file1.pdf",
    //       },
    //       {
    //         path: "egress/folder1/file2.pdf",
    //       },
    //       {
    //         path: "egress/folder1/folder22/file1.pdf",
    //       },

    //       {
    //         path: "egress/file10.pdf",
    //       },
    //     ],
    //     errors: [
    //       {
    //         path: "egress/folder1/file3.pdf",
    //       },
    //       {
    //         path: "egress/folder1/file7.pdf",
    //       },
    //     ],
    //   },
    // },
    // {
    //   id: "5",
    //   actionType: "TRANSFER_INITIATED",
    //   timestamp: "2024-01-15T10:46:10.865517Z",
    //   userId: "dwight_schrute@cps.gov.uk",
    //   userName: "David",
    //   caseId: "case_1",
    //   description: "Document/folders copying from egress to shared drive",
    //   details: {
    //     transferId: "transfer-1",
    //     sourceSystem: "egress",
    //     destinationSystem: "netapp",
    //     fileCount: 2,
    //     sourcePath: "egress",
    //     destinationPath: "netapp/folder2",
    //     files: [],
    //     errors: [],
    //   },
    // },
    {
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
    },
    {
      id: "3",
      actionType: "TRANSFER_INITIATED",
      timestamp: "2024-01-18T12:46:10.865517Z",
      userName: "dwight_schrute@cps.gov.uk",
      caseId: "case_1",
      description: "Document/folders copying from egress to shared drive",
      details: {
        transferId: "transfer-1",
        totalFiles: 2,
        errorFileCount: 0,
        transferedFileCount: 0,
        sourcePath: "egress",
        destinationPath: "netapp/folder2",
        files: [],
        errors: [],
        deletionErrors: [],
      },
    },
    {
      id: "2",
      actionType: "CONNECTION_TO_NETAPP",
      timestamp: "2024-01-18T12:46:10.865517Z",
      userName: "dwight_schrute@cps.gov.uk",
      caseId: "case_1",
      description: "Case connected to the Shared drive",
      details: null,
    },
    {
      id: "1",
      actionType: "CONNECTION_TO_EGRESS",
      timestamp: "2024-01-18T12:46:10.865517Z",
      userName: "dwight_schrute@cps.gov.uk",
      caseId: "case_1",
      description: "Case connected to Egress",
      details: null,
    },
  ],
};

// export const activityLogDev: ActivityLogResponse = {
//   data: [
//     {
//       id: "2",
//       actionType: "CONNECTION_TO_NETAPP",
//       timestamp: "2025-06-18T12:46:10.865517Z",
//       userId: "dwight_schrute@cps.gov.uk",
//       userName: "David",
//       caseId: "case_1",
//       description: "Case connected to the Shared drive",
//       details: null,
//     },
//     {
//       id: "1",
//       actionType: "CONNECTION_TO_EGRESS",
//       timestamp: "2025-06-18T12:46:10.865517Z",
//       userId: "dwight_schrute@cps.gov.uk",
//       userName: "David",
//       caseId: "case_1",
//       description: "Case connected to Egress",
//       details: null,
//     },
//   ],
// };
