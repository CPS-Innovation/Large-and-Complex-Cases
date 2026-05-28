import { z } from "zod";

const batchOperationItemSchema = z.object({
  type: z.enum(["Material", "Folder"]),
  sourcePath: z.string(),
});

export const batchOperationPayloadSchema = z.object({
  caseId: z.number(),
  destinationPrefix: z.string(),
  operations: z.array(batchOperationItemSchema),
});

export type BatchOperationItem = z.infer<typeof batchOperationItemSchema>;
export type BatchOperationPayload = z.infer<typeof batchOperationPayloadSchema>;
