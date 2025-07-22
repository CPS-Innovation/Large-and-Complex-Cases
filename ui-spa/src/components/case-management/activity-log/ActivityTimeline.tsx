import { useState } from "react";
import {
  ActivityLogResponse,
  ActivityItem,
} from "../../../common/types/ActivityLogResponse";
import { Details, Tag, Button } from "../../govuk";
import RelativePathFiles from "./RelativePathFiles";
import { formatDate } from "../../../common/utils/formatDate";
import { getCleanPath } from "../../../common/utils/getCleanPath";
import { getTransferActivityStatusTagData } from "../../../common/utils/getTransferActivityStatusTagData";
import { downloadActivityLog } from "../../../apis/gateway-api";
import styles from "./activityTimeline.module.scss";

type ActivityTimelineProps = {
  activities: ActivityLogResponse;
};

export const ActivityTimeline: React.FC<ActivityTimelineProps> = ({
  activities,
}) => {
  const [downLoadTooltipText, setDownLoadTooltipText] = useState("");

  const showDownloadResult = (text: string) => {
    setDownLoadTooltipText(text);
    setTimeout(() => {
      setDownLoadTooltipText("");
    }, 1000);
  };

  const getTransferStatusTag = (activity: ActivityItem) => {
    const statusTagData = getTransferActivityStatusTagData(activity);
    if (!statusTagData) return <></>;
    return (
      <Tag
        gdsTagColour={statusTagData.color}
        className={styles.transferStatusTag}
        data-testid="transfer-status-tag"
      >
        {statusTagData.name}
      </Tag>
    );
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
      a.download = `activity-log-${activityId}-files.csv`;
      document.body.appendChild(a);
      a.click();
      a.remove();
      window.URL.revokeObjectURL(url);
      showDownloadResult("File list successfully downloaded");
    } catch (error) {
      console.error("Failed to download activity log:", error);
      showDownloadResult("File list download failed");
    }
  };
  return (
    <div
      className={styles.activitiesTimeline}
      data-testid="activities-timeline"
    >
      {activities?.data.map((activity) => (
        <section className={styles.activityWrapper} key={activity.id}>
          <div className={styles.activityHead}></div>
          <div className={styles.activityTitle}>
            <div className={styles.activityTransferTags}>
              {getTransferTag(activity)}
              {getTransferStatusTag(activity)}
            </div>
            <div className={styles.descriptionWrapper}>
              <h4>{activity.description}</h4>{" "}
              <span className={styles.userId} data-testid="activity-user">
                by {activity.userName}
              </span>
            </div>
          </div>

          <span className={styles.activityDate} data-testid="activity-date">
            {formatDate(activity.timestamp, true)}
          </span>

          {activity.details && (
            <div>
              <div>
                <div
                  className={styles.locationData}
                  data-testid="transfer-source"
                >
                  <span className={styles.locationTitle}>Source:</span>
                  <span className={styles.locationPath}>
                    {getCleanPath(activity.details.sourcePath).replace(
                      /\//g,
                      " > ",
                    )}
                  </span>
                </div>
                <div
                  className={styles.locationData}
                  data-testid="transfer-destination"
                >
                  <span className={styles.locationTitle}>Destination:</span>
                  <span className={styles.locationPath}>
                    {getCleanPath(activity.details.destinationPath).replace(
                      /\//g,
                      " > ",
                    )}
                  </span>
                </div>
              </div>

              {(!!activity.details.files.length ||
                !!activity.details.errors.length) && (
                <div>
                  <p>Below is a list of documents/folders copied:</p>
                  <Details summaryChildren="View files">
                    <RelativePathFiles
                      successFiles={activity.details.files}
                      errorFiles={activity.details.errors}
                      sourcePath={activity.details.sourcePath}
                    />
                  </Details>
                  <div className={styles.downloadBtnWrapper}>
                    <Button
                      className={styles.downloadBtn}
                      onClick={() => handleDownload(activity.id)}
                    >
                      Download the list of files (.csv)
                    </Button>
                    {downLoadTooltipText && (
                      <div className={styles.tooltip}>
                        <span>{downLoadTooltipText}</span>
                      </div>
                    )}
                  </div>
                </div>
              )}
            </div>
          )}
        </section>
      ))}
    </div>
  );
};
