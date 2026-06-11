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
  id: z.string().uuid(),
  status: z.enum([
    "Initiated",
    "InProgress",
    "Completed",
    "PartiallyCompleted",
    "Failed",
  ]),
  transferType: z.enum(["Copy", "Move"]),
  direction: z.enum(["EgressToNetApp", "NetAppToEgress"]),
  startedAt: z.string().nullable(),
  completedAt: z.string().nullable(),
  failedItems: z.array(transferFailedItemSchema),
  userName: z.string(),
  totalFiles: z.number(),
  processedFiles: z.number(),
  successfulFiles: z.number(),
  failedFiles: z.number(),
  successItems: z
    .array(
      z.object({
        path: z.string(),
      }),
    )
    .optional(),
  destinationFolderName: z.string().optional(),
  destinationPath: z.string().optional(),
  destinationId: z.string().optional(),
});

export type TransferStatusResponse = z.infer<
  typeof transferStatusResponseSchema
>;
export type TransferFailedItem = z.infer<typeof transferFailedItemSchema>;
