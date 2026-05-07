import { z } from "zod";

export function isTransferDetails(
  details: ActivityDetails | null,
): details is TransferDetails {
  return transferDetailsSchema.safeParse(details).success;
}

export function isBatchDeleteDetails(
  details: ActivityDetails | null,
): details is BatchDeleteDetails {
  return batchDeleteDetailsSchema.safeParse(details).success;
}

export function isBatchCopyDetails(
  details: ActivityDetails | null,
): details is BatchCopyDetails {
  return batchCopyDetailsSchema.safeParse(details).success;
}

export const transferDetailsSchema = z.object({
  transferId: z.string(),
  sourcePath: z.string(),
  destinationPath: z.string(),
  transferType: z.enum(["Move", "Copy"]),
  errorFileCount: z.number(),
  transferedFileCount: z.number(),
  totalFiles: z.number(),
  files: z.array(z.object({ path: z.string() })),
  errors: z.array(z.object({ path: z.string() })),
  deletionErrors: z.array(z.object({ path: z.string() })),
});

export const batchDeleteItemSchema = z.object({
  sourcePath: z.string(),
  outcome: z.string(),
  error: z.string().nullable(),
  keysDeleted: z.number().nullable(),
});

export const batchDeleteDetailsSchema = z.object({
  items: z.array(batchDeleteItemSchema),
});

export const batchCopyItemSchema = z.object({
  sourcePath: z.string(),
  destinationPath: z.string().optional(),
  outcome: z.string(),
  type: z.string(),
});

export const batchCopyDetailsSchema = z.object({
  items: z.array(batchCopyItemSchema),
});

export const activityDetailsSchema = z.union([
  transferDetailsSchema,
  batchDeleteDetailsSchema,
  batchCopyDetailsSchema,
]);

export const activityItemSchema = z.object({
  id: z.string(),
  actionType: z.string(),
  timestamp: z.string(),
  userName: z.string(),
  caseId: z.number(),
  description: z.string(),
  resourceType: z.string().optional(),
  resourceName: z.string().nullable(),
  details: activityDetailsSchema.nullable(),
});

export const activityLogResponseSchema = z.object({
  data: z.array(activityItemSchema),
});

export type TransferDetails = z.infer<typeof transferDetailsSchema>;
export type BatchDeleteItem = z.infer<typeof batchDeleteItemSchema>;
export type BatchDeleteDetails = z.infer<typeof batchDeleteDetailsSchema>;
export type BatchCopyItem = z.infer<typeof batchCopyItemSchema>;
export type BatchCopyDetails = z.infer<typeof batchCopyDetailsSchema>;
export type ActivityItem = z.infer<typeof activityItemSchema>;
export type ActivityLogResponse = z.infer<typeof activityLogResponseSchema>;
export type ActivityDetails =
  | TransferDetails
  | BatchDeleteDetails
  | BatchCopyDetails;
