import { z } from "zod";

export const transferFailedItemSchema = z.object({
  sourcePath: z.string(),
  errorCode: z.enum([
    "FileExists",
    "GeneralError",
    "IntegrityVerificationFailed",
    "Transient",
  ]),
});

export const transferStatusResponseSchema = z.object({
  id: z.string().uuid().optional(),
  status: z.enum([
    "Initiated",
    "InProgress",
    "Completed",
    "PartiallyCompleted",
    "Failed",
  ]),
  transferType: z.enum(["Copy", "Move"]),
  direction: z.enum(["EgressToNetApp", "NetAppToEgress"]),
  startedAt: z.string().nullable().optional(),
  completedAt: z.string().nullable(),
  failedItems: z.array(transferFailedItemSchema),
  userName: z.string(),
  totalFiles: z.number(),
  processedFiles: z.number(),
  successfulFiles: z.number().optional(),
  failedFiles: z.number().optional(),
});

export type TransferStatusResponse = z.infer<
  typeof transferStatusResponseSchema
>;
export type TransferFailedItem = z.infer<typeof transferFailedItemSchema>;
