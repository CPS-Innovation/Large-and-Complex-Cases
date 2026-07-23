import { type ActivityItem, isTransferDetails } from "../../schemas";

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
  const skippedFileCount = details.skippedFileCount ?? 0;
  const accountedFor = details.transferedFileCount + skippedFileCount;

  // Skip-only batches (e.g. empty files to Egress) still complete successfully.
  if (!accountedFor)
    return {
      name: "Failed",
      color: "red",
    };

  if (details.totalFiles === accountedFor && !details.errorFileCount)
    return {
      name: "Completed",
      color: "green",
    };

  return {
    name: "Completed with errors",
    color: "yellow",
  };
};
