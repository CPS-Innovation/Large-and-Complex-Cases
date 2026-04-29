import {
  ActivityItem,
  isTransferDetails,
} from "../../schemas/responses/activityLogResponse";

export const getTransferActivityStatusTagData = (
  activity: ActivityItem,
): {
  name: "Completed" | "Failed" | "Completed with errors";
  color: "green" | "red" | "yellow";
} | null => {
  if (
    activity.actionType !== "TRANSFER_COMPLETED" &&
    activity.actionType !== "TRANSFER_FAILED"
  )
    return null;

  if (!isTransferDetails(activity.details)) return null;

  const { details } = activity;

  if (!details.transferedFileCount)
    return {
      name: "Failed",
      color: "red",
    };

  if (
    details.totalFiles === details.transferedFileCount &&
    !details.errorFileCount
  )
    return {
      name: "Completed",
      color: "green",
    };

  return {
    name: "Completed with errors",
    color: "yellow",
  };
};
