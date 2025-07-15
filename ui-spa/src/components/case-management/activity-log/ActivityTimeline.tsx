import { ActivityLogResponse } from "../../../common/types/ActivityLogResponse";
import { Details } from "../../govuk";
import RelativePathFiles from "./RelativePathFiles";
import { formatDate } from "../../../common/utils/formatDate";
import styles from "./activityTimeline.module.scss";

type ActivityTimelineProps = {
  activities: ActivityLogResponse;
};

export const ActivityTimeline: React.FC<ActivityTimelineProps> = ({
  activities,
}) => {
  return (
    <section
      className={styles.activitiesTimeline}
      data-testid="activities-list"
    >
      {activities?.items.toReversed().map((activity) => (
        <div className={styles.activityWrapper} key={activity.id}>
          <div className={styles.activityHead}>
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
                  <span> {activity.details.sourcePath}</span>
                </div>
                <div>
                  <span> Destination:</span>
                  <span> {activity.details.destinationPath}</span>
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
                </div>
              )}
            </div>
          )}
        </div>
      ))}
    </section>
  );
};
