import { z } from "zod";

export const batchOperationResponseSchema = z.object({
  id: z.string(),
  status: z.string(),
  createdAt: z.string(),
});

export type BatchOperationResponse = z.infer<typeof batchOperationResponseSchema>;
