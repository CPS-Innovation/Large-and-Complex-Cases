import { ActivityTimeline } from "./ActivityTimeline";
import { ActivityLogResponse } from "../../../common/types/ActivityLogResponse";
import styles from "./index.module.scss";

const ActvityLogPageProps = () => {
  const activities: ActivityLogResponse = {
    items: [
      {
        id: "1",
        actionType: "CONNECTION_TO_EGRESS",
        timestamp: "2025-06-18T12:46:10.865517Z",
        userId: "dwight_schrute@cps.gov.uk",
        userName: "David",
        caseId: "case_1",
        description: " Case connected to Egress",
        details: null,
      },
      {
        id: "2",
        actionType: "CONNECTION_TO_NETAPP",
        timestamp: "2025-06-18T12:46:10.865517Z",
        userId: "dwight_schrute@cps.gov.uk",
        userName: "David",
        caseId: "case_1",
        description: " Case connected to the Shared drive",
        details: null,
      },
      {
        id: "3",
        actionType: "TRANSFER_INITIATED",
        timestamp: "2025-06-18T12:46:10.865517Z",
        userId: "dwight_schrute@cps.gov.uk",
        userName: "David",
        caseId: "case_1",
        description: "Transfer from egress to shared drive",
        details: {
          transferId: "transfer-1",
          sourceSystem: "egress",
          destinationSystem: "netapp",
          fileCount: 2,
          sourcePath: "egress",
          destinationPath: "netapp/folder2",
          files: [],
          errors: [],
        },
      },
      {
        id: "4",
        actionType: "TRANSFER_COMPLETED",
        timestamp: "2025-06-18T12:46:10.865517Z",
        userId: "dwight_schrute@cps.gov.uk",
        userName: "David",
        caseId: "case_1",
        description: "Transfer from egress to shared drive",
        details: {
          transferId: "transfer-1",
          sourceSystem: "egress",
          destinationSystem: "netapp",
          fileCount: 2,
          sourcePath: "egress",
          destinationPath: "netapp/folder2",
          files: [
            {
              sourcePath: "egress/folder1/file1.pdf",
            },
            {
              sourcePath: "egress/folder1/file2.pdf",
            },
            {
              sourcePath: "egress/folder1/folder22/file1.pdf",
            },
            {
              sourcePath: "egress/file10.pdf",
            },
          ],
          errors: [
            {
              sourcePath: "egress/folder1/file3.pdf",
            },
            {
              sourcePath: "egress/folder1/file7.pdf",
            },
          ],
        },
      },
      {
        id: "5",
        actionType: "TRANSFER_INITIATED",
        timestamp: "2025-07-15T10:46:10.865517Z",
        userId: "dwight_schrute@cps.gov.uk",
        userName: "David",
        caseId: "case_1",
        description: "Transfer from egress to shared drive",
        details: {
          transferId: "transfer-1",
          sourceSystem: "egress",
          destinationSystem: "netapp",
          fileCount: 2,
          sourcePath: "egress",
          destinationPath: "netapp/folder2",
          files: [],
          errors: [],
        },
      },
      {
        id: "6",
        actionType: "TRANSFER_FAILED",
        timestamp: "2025-07-15T11:45:10.865517Z",
        userId: "dwight_schrute@cps.gov.uk",
        userName: "David",
        caseId: "case_1",
        description: "Transfer from egress to shared drive",
        details: {
          transferId: "transfer-1",
          sourceSystem: "egress",
          destinationSystem: "netapp",
          fileCount: 2,
          sourcePath: "egress",
          destinationPath: "netapp/folder2",
          files: [
            {
              sourcePath: "egress/folder1/file1.pdf",
            },
            {
              sourcePath: "egress/folder1/file2.pdf",
            },
            {
              sourcePath: "egress/folder1/folder22/file1.pdf",
            },
            {
              sourcePath: "egress/file10.pdf",
            },
          ],
          errors: [
            {
              sourcePath: "egress/folder1/file3.pdf",
            },
            {
              sourcePath: "egress/folder1/file7.pdf",
            },
          ],
        },
      },
    ],
  };

  return (
    <div>
      <h2>Activity Log</h2>

      <div className={styles.activities}>
        <div className={styles.titleWrapper}>
          <h3>Activity</h3>
        </div>
        <div className={styles.contentWrapper}>
          <ActivityTimeline activities={activities} />
        </div>
      </div>
    </div>
  );
};

export default ActvityLogPageProps;
