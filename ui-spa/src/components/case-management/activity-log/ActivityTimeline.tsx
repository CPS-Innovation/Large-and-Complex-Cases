import { useState } from "react";
import {
  type ActivityLogResponse,
  type ActivityItem,
  type BatchDeleteDetails,
  type BatchCopyDetails,
  type BatchMoveDetails,
  type TransferDetails,
  isBatchCopyDetails,
  isBatchDeleteDetails,
  isBatchMoveDetails,
  isTransferDetails,
} from "../../../schemas";
import { Details, Tag, Button } from "../../govuk";
import RelativePathFiles from "./RelativePathFiles";
import { formatDate } from "../../../common/utils/formatDate";
import { formatInTimeZone } from "date-fns-tz";
import { getCleanPath } from "../../../common/utils/getCleanPath";
import { getTransferActivityStatusTagData } from "../../../common/utils/getTransferActivityStatusTagData";
import { downloadActivityLog } from "../../../apis/gateway-api";
import styles from "./ActivityTimeline.module.scss";

type ActivityTimelineProps = {
  operationName: string;
  activities: ActivityLogResponse;
};

export const ActivityTimeline: React.FC<ActivityTimelineProps> = ({
  activities,
  operationName,
}) => {
  const [downloadTooltipTexts, setDownloadTooltipTexts] = useState<
    Record<string, string>
  >({});

  const showDownloadResult = (activityId: string, text: string) => {
    setDownloadTooltipTexts({ [`${activityId}`]: text });
    setTimeout(() => {
      setDownloadTooltipTexts({ [`${activityId}`]: "" });
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

  const handleDownload = async (activityId: string, timestamp: string) => {
    const formattedTime = formatInTimeZone(
      timestamp,
      "Europe/London",
      "dd-MM-yyyy-h-mm-aa",
    );
    try {
      const response = await downloadActivityLog(activityId);
      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = `activity-log-files-${operationName}-${formattedTime}.csv`;
      document.body.appendChild(a);
      a.click();
      a.remove();
      window.URL.revokeObjectURL(url);
      showDownloadResult(activityId, "File list successfully downloaded");
    } catch (error) {
      console.error("Failed to download activity log:", error);
      showDownloadResult(activityId, "File list download failed");
    }
  };

  const renderTransferDetails = (
    activityId: string,
    timestamp: string,
    details: TransferDetails,
  ) => (
    <div>
      <div>
        <div className={styles.locationData} data-testid="transfer-source">
          <span className={styles.locationTitle}>Source:</span>
          <span className={styles.locationPath}>
            {getCleanPath(details.sourcePath).replace(/\//g, " > ")}
          </span>
        </div>
        <div className={styles.locationData} data-testid="transfer-destination">
          <span className={styles.locationTitle}>Destination:</span>
          <span className={styles.locationPath}>
            {getCleanPath(details.destinationPath).replace(/\//g, " > ")}
          </span>
        </div>
      </div>

      {(!!details.files.length || !!details.errors.length) && (
        <div>
          <p>Below is a list of documents/folders copied:</p>
          <Details summaryChildren="View files">
            <RelativePathFiles
              successFiles={details.files}
              errorFiles={details.errors}
              sourcePath={details.sourcePath}
            />
          </Details>
          <div className={styles.downloadBtnWrapper}>
            <Button
              className={styles.downloadBtn}
              onClick={() => handleDownload(activityId, timestamp)}
            >
              Download the list of files (.csv)
            </Button>
            {downloadTooltipTexts[`${activityId}`] && (
              <div
                className={styles.tooltip}
                data-testid="activity-download-tooltip"
              >
                <span>{downloadTooltipTexts[`${activityId}`]}</span>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );

  const renderBatchDeleteDetails = (details: BatchDeleteDetails) => (
    <div>
      <Details summaryChildren="View deleted items">
        <ul className={styles.batchDeleteList}>
          {details.items.map((item, i) => (
            <li key={i} className={styles.batchDeleteItem}>
              <span className={styles.batchDeletePath}>
                {getCleanPath(item.sourcePath).replace(/\//g, " > ")}
              </span>
              <Tag
                gdsTagColour={
                  item.outcome === "Deleted"
                    ? "green"
                    : item.outcome === "NotFound"
                      ? "grey"
                      : "red"
                }
                className={styles.batchDeleteTag}
                data-testid="batch-delete-outcome-tag"
              >
                {item.outcome}
              </Tag>
              {item.error && (
                <span
                  className={styles.batchDeleteError}
                  data-testid="batch-delete-error"
                >
                  {item.error}
                </span>
              )}
            </li>
          ))}
        </ul>
      </Details>
    </div>
  );

  const renderBatchCopyDetails = (details: BatchCopyDetails) => (
    <div>
      <Details summaryChildren="View copied items">
        <ul className={styles.batchDeleteList}>
          {details.items.map((item, i) => (
            <li key={i} className={styles.batchCopyItem}>
              <div className={styles.batchCopyPaths}>
                <div data-testid="batch-copy-source">
                  <span className={styles.locationTitle}>Source:</span>
                  <span className={styles.locationPath}>
                    {getCleanPath(item.sourcePath).replace(/\//g, " > ")}
                  </span>
                </div>
                {item.destinationPath && (
                  <div data-testid="batch-copy-destination">
                    <span className={styles.locationTitle}>Destination:</span>
                    <span className={styles.locationPath}>
                      {getCleanPath(item.destinationPath).replace(/\//g, " > ")}
                    </span>
                  </div>
                )}
              </div>
              <Tag
                gdsTagColour={item.outcome === "Copied" ? "green" : "grey"}
                className={styles.batchDeleteTag}
                data-testid="batch-copy-outcome-tag"
              >
                {item.outcome}
              </Tag>
            </li>
          ))}
        </ul>
      </Details>
    </div>
  );

  const renderBatchMoveDetails = (details: BatchMoveDetails) => (
    <div>
      <Details summaryChildren="View moved items">
        <ul className={styles.batchDeleteList}>
          {details.items.map((item, i) => (
            <li key={i} className={styles.batchCopyItem}>
              <div className={styles.batchCopyPaths}>
                <div data-testid="batch-move-source">
                  <span className={styles.locationTitle}>Source:</span>
                  <span className={styles.locationPath}>
                    {getCleanPath(item.sourcePath).replace(/\//g, " > ")}
                  </span>
                </div>
                {item.destinationPath && (
                  <div data-testid="batch-move-destination">
                    <span className={styles.locationTitle}>Destination:</span>
                    <span className={styles.locationPath}>
                      {getCleanPath(item.destinationPath).replace(/\//g, " > ")}
                    </span>
                  </div>
                )}
              </div>
              <Tag
                gdsTagColour={item.outcome === "Moved" ? "green" : "grey"}
                className={styles.batchDeleteTag}
                data-testid="batch-move-outcome-tag"
              >
                {item.outcome}
              </Tag>
            </li>
          ))}
        </ul>
      </Details>
    </div>
  );

  const MOVE_ACTION_TYPES = ["FOLDER_MOVED", "MATERIAL_MOVED", "FOLDER_AND_MATERIAL_MOVED"];

  const renderDetails = (activity: ActivityItem) => {
    const { details } = activity;
    if (!details) return null;

    if (isTransferDetails(details)) {
      return renderTransferDetails(activity.id, activity.timestamp, details);
    }

    if (MOVE_ACTION_TYPES.includes(activity.actionType) && isBatchMoveDetails(details)) {
      return renderBatchMoveDetails(details);
    }

    if (isBatchCopyDetails(details)) {
      return renderBatchCopyDetails(details);
    }

    if (isBatchDeleteDetails(details)) {
      return renderBatchDeleteDetails(details);
    }

    return null;
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

          {renderDetails(activity)}
        </section>
      ))}
    </div>
  );
};
