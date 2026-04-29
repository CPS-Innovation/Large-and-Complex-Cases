import { z } from "zod";

export function isTransferDetails(
  details: TransferDetails | BatchDeleteDetails | null,
): details is TransferDetails {
  return transferDetailsSchema.safeParse(details).success;
}

export function isBatchDeleteDetails(
  details: TransferDetails | BatchDeleteDetails | null,
): details is BatchDeleteDetails {
  return batchDeleteDetailsSchema.safeParse(details).success;
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

export const activityItemSchema = z.object({
  id: z.string(),
  actionType: z.string(),
  timestamp: z.string(),
  userName: z.string(),
  caseId: z.string(),
  description: z.string(),
  resourceType: z.string().optional(),
  resourceName: z.string().optional(),
  details: z
    .union([transferDetailsSchema, batchDeleteDetailsSchema])
    .nullable(),
});

export const activityLogResponseSchema = z.object({
  data: z.array(activityItemSchema),
});

export type TransferDetails = z.infer<typeof transferDetailsSchema>;
export type BatchDeleteItem = z.infer<typeof batchDeleteItemSchema>;
export type BatchDeleteDetails = z.infer<typeof batchDeleteDetailsSchema>;
export type ActivityItem = z.infer<typeof activityItemSchema>;
export type ActivityLogResponse = z.infer<typeof activityLogResponseSchema>;
