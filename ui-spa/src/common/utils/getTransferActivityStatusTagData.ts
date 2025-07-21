import { ActivityItem } from "../types/ActivityLogResponse";

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

  if (!activity.details) return null;
  if (!activity.details.transferedFileCount)
    return {
      name: "Failed",
      color: "red",
    };

  if (
    activity.details.totalFiles === activity.details.transferedFileCount &&
    !activity.details.errorFileCount
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
