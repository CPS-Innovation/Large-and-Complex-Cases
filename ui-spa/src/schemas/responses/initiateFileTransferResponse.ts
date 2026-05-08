import { z } from "zod";

export const initiateFileTransferResponseSchema = z.object({
  id: z.string(),
});

export type InitiateFileTransferResponse = z.infer<
  typeof initiateFileTransferResponseSchema
>;
