import { useEffect } from "react";
import { ActivityTimeline } from "./ActivityTimeline";
import { useApi } from "../../../common/hooks/useApi";
import { getActivityLog } from "../../../apis/gateway-api";
import { useParams } from "react-router-dom";
import styles from "./index.module.scss";

const ActvityLogPageProps = () => {
  const { caseId } = useParams() as { caseId: string };
  const activityLogResponse = useApi(getActivityLog, [caseId], true);

  console.log(activityLogResponse)

  useEffect(() => {
    if (activityLogResponse.status === "failed")
      throw new Error(`${activityLogResponse.error}`);
  }, [activityLogResponse]);
  return (
    <div>
      <h2>Activity Log</h2>

      <div className={styles.activities}>
        <div className={styles.titleWrapper}>
          <h3>Activity</h3>
        </div>
        <div className={styles.contentWrapper}>
          {activityLogResponse?.data && (
            <ActivityTimeline activities={activityLogResponse.data} />
          )}
        </div>
      </div>
    </div>
  );
};

export default ActvityLogPageProps;
