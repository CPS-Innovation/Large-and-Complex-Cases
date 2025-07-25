import { useEffect } from "react";
import { ActivityTimeline } from "./ActivityTimeline";
import { useApi } from "../../../common/hooks/useApi";
import { getActivityLog } from "../../../apis/gateway-api";
import { useParams } from "react-router-dom";
import { formatDate } from "../../../common/utils/formatDate";
import styles from "./index.module.scss";

type ActivityLogPageProps = {
  operationName: string;
  isTabActive: boolean;
};

const ActivityLogPage: React.FC<ActivityLogPageProps> = ({
  isTabActive,
  operationName,
}) => {
  const { caseId } = useParams() as { caseId: string };
  const activityLogResponse = useApi(getActivityLog, [caseId], isTabActive);

  const getLastUpdatedText = () => {
    if (!activityLogResponse?.data) return "";
    const { data: items } = activityLogResponse.data;
    if (!items) return "";
    const { timestamp } = items[0];
    return <span> Last Updated {formatDate(timestamp, true)}</span>;
  };

  useEffect(() => {
    if (activityLogResponse.status === "failed")
      throw new Error(`${activityLogResponse.error}`);
  }, [activityLogResponse]);

  if (!isTabActive) return <> </>;
  return (
    <div>
      <div className={styles.titleText}>
        <h2>Activity Log</h2>
        <span
          className={styles.lastUpdatedText}
          data-testid={"activity-log-last-update"}
        >
          {getLastUpdatedText()}
        </span>
      </div>
      {activityLogResponse?.data && (
        <div className={styles.activities}>
          <div className={styles.titleWrapper}>
            <h3>Activity</h3>
          </div>
          <div className={styles.contentWrapper}>
            <ActivityTimeline
              activities={activityLogResponse.data}
              operationName={operationName}
            />
          </div>
        </div>
      )}
    </div>
  );
};

export default ActivityLogPage;
