import {
  ActivityLogResponse,
  ActivityItem,
} from "../../../common/types/ActivityLogResponse";
import { Details, Tag, Button } from "../../govuk";
import RelativePathFiles from "./RelativePathFiles";
import { formatDate } from "../../../common/utils/formatDate";
import styles from "./activityTimeline.module.scss";
import { downloadActivityLog } from "../../../apis/gateway-api";

type ActivityTimelineProps = {
  activities: ActivityLogResponse;
};

export const ActivityTimeline: React.FC<ActivityTimelineProps> = ({
  activities,
}) => {
  console.log("activities,>>>", activities);
  const getTransferStatusTag = (activity: ActivityItem) => {
    if (
      activity.actionType === "TRANSFER_COMPLETED" ||
      activity.actionType === "TRANSFER_FAILED"
    ) {
      console.log("ctivity.actionType >>", activity.actionType);
      if (activity.details?.Errors.length && activity.details?.Files.length)
        return (
          <Tag
            gdsTagColour="yellow"
            className={styles.transferStatusTag}
            data-testid="transfer-status-tag"
          >
            Completed with errors
          </Tag>
        );

      if (!activity.details?.Errors.length)
        return (
          <Tag
            gdsTagColour="green"
            className={styles.transferStatusTag}
            data-testid="transfer-status-tag"
          >
            Completed
          </Tag>
        );
      if (activity.details?.Errors.length)
        return (
          <Tag
            gdsTagColour="red"
            className={styles.transferStatusTag}
            data-testid="transfer-status-tag"
          >
            Failed
          </Tag>
        );
    }
  };

  const getTransferTag = (activity: ActivityItem) => {
    if (
      activity.actionType === "TRANSFER_COMPLETED" ||
      activity.actionType === "TRANSFER_FAILED" ||
      activity.actionType === "TRANSFER_INITIATED"
    ) {
      return (
        <Tag
          gdsTagColour="grey"
          className={styles.transferTag}
          data-testid="transfer-tag"
        >
          Transfer
        </Tag>
      );
    }
  };

  const handleDownload = async (activityId: string) => {
    try {
      const response = await downloadActivityLog(activityId);
      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      // Use filename from API if available, else fallback
      a.download = `activity-log-${activityId}-files.csv`;
      document.body.appendChild(a);
      a.click();
      a.remove();
      window.URL.revokeObjectURL(url);
    } catch (error) {
      // Optionally show error to user
      console.error("Failed to download activity log:", error);
    }
  };

  return (
    <section
      className={styles.activitiesTimeline}
      data-testid="activities-list"
    >
      {activities?.toReversed().map((activity) => (
        <div className={styles.activityWrapper} key={activity.id}>
          <div className={styles.activityHead}></div>
          <div className={styles.activityTitle}>
            <div className={styles.activityTransferTags}>
              {getTransferTag(activity)}
              {getTransferStatusTag(activity)}
            </div>
            <b>{activity.description}</b>{" "}
            <span className={styles.userId}>by {activity.userId}</span>
          </div>

          <span className={styles.activityDate}>
            {formatDate(activity.timestamp, true)}
          </span>

          {activity.details && (
            <div>
              <div>
                <div>
                  <span> Source:</span>
                  <span> {activity.details.SourcePath}</span>
                </div>
                <div>
                  <span> Destination:</span>
                  <span> {activity.details.DestinationPath}</span>
                </div>
              </div>

              {(!!activity.details.Files.length ||
                !!activity.details.Errors.length) && (
                <div>
                  <p>Below is a list of documents/folders copied:</p>
                  <Details summaryChildren="View files">
                    <RelativePathFiles
                      successFiles={activity.details.Files}
                      errorFiles={activity.details.Errors}
                      sourcePath={activity.details.SourcePath}
                    />
                  </Details>
                  <Button
                    className={styles.downloadBtn}
                    onClick={() => handleDownload(activity.id)}
                  >
                    Download the list of files (.csv)
                  </Button>
                </div>
              )}
            </div>
          )}
        </div>
      ))}
    </section>
  );
};
